import { expect, type Page } from '@playwright/test';

export interface InvoiceState {
  id: string;
  customerName: string;
  amount: number;
  status: string;
  createdAt: string;
  firstApprovedBy: string | null;
  firstApprovedRole: string | null;
  firstApprovedAt: string | null;
  decidedBy: string | null;
  decidedAt: string | null;
  decisionNote: string | null;
}

export class InvoiceAppPage {
  constructor(readonly page: Page) {}

  async signInAsManager() {
    await this.page.goto('/');
    await this.page.getByLabel('Username', { exact: true }).fill('demo.approver');
    await this.page.getByLabel('Password', { exact: true }).fill('ChangeMe123!');
    await this.page.getByRole('button', { name: 'Sign in', exact: true }).click();
    await expect(this.page.getByRole('button', { name: 'Sign out', exact: true })).toBeVisible();
    // Actual UI login; no injected session and no mocked API.
    await expect.poll(() => this.page.evaluate(() => {
      const session = JSON.parse(sessionStorage.getItem('session') || 'null');
      return { username: session?.username, role: session?.role };
    })).toEqual({ username: 'demo.approver', role: 'Manager' });
  }

  async createInvoice(customerName: string, amount: string): Promise<string> {
    await this.page.getByLabel('Customer name', { exact: true }).fill(customerName);
    await this.page.getByLabel('Amount', { exact: true }).fill(amount);
    const created = this.page.waitForResponse(response =>
      new URL(response.url()).pathname === '/api/invoices' && response.request().method() === 'POST');
    await this.page.getByRole('button', { name: 'Create invoice', exact: true }).click();
    const response = await created;
    expect(response.status()).toBe(201);
    const invoice: InvoiceState = await response.json();
    await expect(this.invoiceRow(invoice.id).getByRole('cell', { name: customerName, exact: true })).toBeVisible();
    return invoice.id;
  }

  invoiceRow(id: string) {
    return this.page.getByRole('table', { name: 'All invoices' }).getByRole('row')
      .filter({ has: this.page.getByRole('cell', { name: id, exact: true }) });
  }

  async submitInvoice(id: string) {
    await this.page.getByRole('button', { name: `Submit invoice ${id} for approval`, exact: true }).click();
    await this.expectPending(id);
  }

  async expectPending(id: string) {
    await expect(this.invoiceRow(id).getByRole('cell', { name: 'Pending Approval', exact: true })).toBeVisible();
  }

  async persistedInvoice(id: string): Promise<InvoiceState> {
    // Token stays inside the browser; only a fresh invoice enters reports.
    return this.page.evaluate(async invoiceId => {
      const session = JSON.parse(sessionStorage.getItem('session') || 'null');
      if (!session?.token) throw new Error('Expected authenticated browser session');
      const response = await fetch(`/api/invoices/${encodeURIComponent(invoiceId)}`, {
        headers: { Authorization: `Bearer ${session.token}` }, cache: 'no-store',
      });
      if (!response.ok) throw new Error(`Invoice read failed: ${response.status}`);
      return response.json();
    }, id);
  }

  async attemptApproval(id: string): Promise<number> {
    await this.page.getByRole('tab', { name: 'Approvals', exact: true }).click();
    const decision = this.page.waitForResponse(response =>
      new URL(response.url()).pathname === `/api/approvals/${id}/approve` && response.request().method() === 'POST');
    await this.page.getByRole('button', { name: `Approve invoice ${id}`, exact: true }).click();
    const response = await decision;
    await expect(this.page.getByRole('alert')).toHaveText(
      'Manager approval limit is USD 50,000 inclusive; a Director is required.');
    return response.status();
  }
}
