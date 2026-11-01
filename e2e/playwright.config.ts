import { defineConfig, devices } from '@playwright/test';
import { execFileSync } from 'node:child_process';
import path from 'node:path';

// Ask the OS for a free loopback port. A competing bind fails the run; it never
// permits reuse of an existing server (especially the participant's port 5000).
const port = process.env.INVOICE_E2E_PORT || execFileSync(process.execPath, ['-e', `
  const server = require('node:net').createServer();
  server.listen(0, '127.0.0.1', () => {
    process.stdout.write(String(server.address().port));
    server.close();
  });
`], { encoding: 'utf8' }).trim();
if (!/^\d+$/.test(port) || Number(port) < 1 || Number(port) > 65535) {
  throw new Error('INVOICE_E2E_PORT must be a port number from 1 through 65535.');
}
// Workers inherit the selected port instead of allocating another one when
// Playwright evaluates this configuration in a child process.
process.env.INVOICE_E2E_PORT = port;
const baseURL = `http://127.0.0.1:${port}`;
const repository = path.resolve(__dirname, '..');
const dotnetRoot = process.env.DOTNET_ROOT || path.join(repository, '.tools/dotnet');

export default defineConfig({
  testDir: './tests',
  workers: 1,
  retries: 0,
  reporter: [
    ['list'],
    ['json', { outputFile: '../docs/evidence/phase-6/playwright/results.json' }],
    ['html', { open: 'never' }],
  ],
  use: {
    baseURL,
    // Network traces can contain the demo session token. Keep them disabled.
    trace: 'off',
    screenshot: 'only-on-failure',
    video: 'off',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: {
    command: `dotnet run --no-launch-profile --project backend/src/InvoiceApp.Api -- --urls ${baseURL}`,
    cwd: repository,
    env: { DOTNET_ROOT: dotnetRoot, PATH: `${dotnetRoot}${path.delimiter}${process.env.PATH || ''}` },
    url: baseURL,
    reuseExistingServer: false,
    timeout: 120_000,
  },
});
