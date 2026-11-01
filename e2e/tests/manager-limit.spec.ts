import { test, expect } from '@playwright/test';
import { randomUUID } from 'node:crypto';
import { mkdir, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { InvoiceAppPage } from '../pages/invoice-app.page';

test('Manager cannot approve USD 50,000.01; persisted invoice stays completely unchanged after reload', async ({ page }, testInfo) => {
  const app = new InvoiceAppPage(page);
  const customerName = `Synthetic phase-six ${randomUUID()}`;
  await app.signInAsManager();
  const id = await app.createInvoice(customerName, '50000.01');
  await app.submitInvoice(id);
  const before = await app.persistedInvoice(id);

  expect(Object.keys(before).sort()).toEqual([
    'id', 'customerName', 'amount', 'status', 'createdAt',
    'firstApprovedBy', 'firstApprovedRole', 'firstApprovedAt',
    'decidedBy', 'decidedAt', 'decisionNote',
  ].sort());
  expect(before).toEqual({
    id, customerName, amount: 50000.01, status: 'PendingApproval',
    createdAt: expect.any(String),
    firstApprovedBy: null, firstApprovedRole: null, firstApprovedAt: null,
    decidedBy: null, decidedAt: null, decisionNote: null,
  });
  expect(Number.isNaN(Date.parse(before.createdAt))).toBe(false);

  const approvalStatus = await app.attemptApproval(id);
  expect(approvalStatus).toBe(409);
  await page.reload();
  await app.expectPending(id);
  const after = await app.persistedInvoice(id);
  // Deep equality includes every returned field; extra future fields fail too.
  expect(after).toEqual(before);

  const evidenceDirectory = path.resolve(__dirname, '../../docs/evidence/phase-6/playwright');
  await mkdir(evidenceDirectory, { recursive: true });
  await writeFile(path.join(evidenceDirectory, 'invoice-state.json'), JSON.stringify({
    capturedAt: new Date().toISOString(),
    baseURL: testInfo.project.use.baseURL,
    approvalHttpStatus: approvalStatus,
    visibleReason: 'Manager approval limit is USD 50,000 inclusive; a Director is required.',
    before, after,
    allFieldsUnchangedAfterReload: true,
  }, null, 2) + '\n');
});
