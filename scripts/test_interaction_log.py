"""Logger checks use temporary fixtures, never the assignment evidence files."""
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

import interaction_log as audit


class InteractionLogTests(unittest.TestCase):
    def test_redacts_nested_credentials_and_text_tokens(self):
        value = {"token": "private", "nested": [{"password": "private"}],
                 "text": 'Authorization: Bearer abc123; {"api_key": "private"} ghp_1234567890abcdefghij'}
        cleaned = json.dumps(audit.sanitize(value))
        self.assertNotIn("private", cleaned)
        self.assertNotIn("abc123", cleaned)
        self.assertNotIn("ghp_1234567890abcdefghij", cleaned)
        self.assertIn("[REDACTED]", cleaned)

    def test_sync_omits_internal_content_and_retries_partial_tail_without_duplicates(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp).resolve() / "repo"
            root.mkdir()
            source = Path(tmp) / "rollout.jsonl"
            events = [
                {"type": "session_meta", "payload": {"id": "fixture", "cwd": str(root)}},
                {"type": "turn_context", "payload": {"model": "observed-model", "turn_id": "turn-1"}},
                {"type": "response_item", "payload": {"type": "message", "role": "system", "content": [{"type": "text", "text": "internal-system"}]}},
                {"type": "response_item", "payload": {"type": "reasoning", "encrypted_content": "internal-reasoning"}},
                {"type": "response_item", "payload": {"type": "message", "role": "assistant", "channel": "analysis", "content": [{"type": "text", "text": "internal-analysis"}]}},
                {"type": "response_item", "payload": {"type": "message", "role": "user", "content": [{"type": "input_text", "text": "real request"}]}},
            ]
            tail = {"type": "response_item", "payload": {"type": "custom_tool_call_output", "call_id": "c1", "output": "actual tool result"}}
            source.write_text("".join(json.dumps(e) + "\n" for e in events) + json.dumps(tail))
            with patch.object(audit, "ROOT", root), patch.object(audit, "LOGS", root / "logs"):
                self.assertEqual(1, audit.sync(source)["exported"])
                self.assertEqual(0, audit.sync(source)["exported"])
                with source.open("a") as stream:
                    stream.write("\n")
                self.assertEqual(1, audit.sync(source)["exported"])
                output = (root / "logs/transcripts/fixture.jsonl").read_text()
                self.assertNotIn("internal-", output)
                self.assertIn("real request", output)
                self.assertIn("actual tool result", output)
                self.assertIn("observed-model", output)

    def test_refuses_transcript_for_unrelated_workspace(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "rollout.jsonl"
            path.write_text(json.dumps({"type": "session_meta", "payload": {"id": "fixture", "cwd": "/unrelated/workspace"}}) + "\n")
            with self.assertRaisesRegex(ValueError, "outside this assignment"):
                audit.sync(path)


if __name__ == "__main__":
    unittest.main()
