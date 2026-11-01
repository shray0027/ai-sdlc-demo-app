const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const path = require('node:path');
const test = require('node:test');
const vm = require('node:vm');
const source = readFileSync(path.join(__dirname, '../../src/InvoiceApp.Api/wwwroot/app.js'), 'utf8');

class Element {
  constructor(tag = '') { this.tag = tag; this.children = []; this.attributes = {}; this.handlers = {}; this.hidden = false; this.value = ''; this.ownText = ''; }
  set textContent(value) { this.ownText = value; this.children = []; }
  get textContent() { return this.ownText + this.children.map(child => typeof child === 'string' ? child : child.textContent).join(''); }
  setAttribute(key, value) { this.attributes[key] = value; }
  addEventListener(event, handler) { this.handlers[event] = handler; }
  append(...children) { this.children.push(...children); }
  replaceChildren(...children) { this.ownText = ''; this.children = children; }
  querySelector(selector) { assert.equal(selector, 'tbody'); return this.body ||= new Element('tbody'); }
  reset() {}
  find(label) {
    if (this.attributes['aria-label'] === label) return this;
    for (const child of this.children) if (child instanceof Element) { const found = child.find(label); if (found) return found; }
  }
}
const settle = () => new Promise(resolve => setImmediate(resolve));
function harness(server, role = 'Manager') {
  const elements = new Map(); const calls = [];
  const get = id => { if (!elements.has(id)) elements.set(id, new Element()); return elements.get(id); };
  vm.runInNewContext(source, {
    document: { getElementById: get, createElement: tag => new Element(tag) },
    sessionStorage: { getItem: () => JSON.stringify({ username: 'browser.user', role, token: 'server-token' }), setItem() {}, removeItem() {} },
    fetch: async (url, options) => {
      calls.push({ url, ...options });
      const result = options.method === 'POST' ? server.approve() : { status: 200, body: [server.invoice] };
      return { status: result.status, ok: result.status < 400, json: async () => structuredClone(result.body) };
    }
  }, { filename: 'app.js' });
  return { get, calls, async approve() { await get('pending-list').find('Approve invoice INV-high').handlers.click(); await settle(); } };
}
const pending = amount => ({ id: 'INV-high', customerName: 'School', amount, status: 'PendingApproval', firstApprovedBy: null, firstApprovedRole: null, firstApprovedAt: null, decidedBy: null, decidedAt: null, decisionNote: null });

test('actual rendering shows server first approval after refresh and fresh app reload, then removes completed pending item', async () => {
  const server = { invoice: pending(500000.01), approve() {
    if (!this.invoice.firstApprovedBy) Object.assign(this.invoice, { firstApprovedBy: 'server.manager', firstApprovedRole: 'Manager', firstApprovedAt: '2026-11-01T10:00:00Z' });
    else Object.assign(this.invoice, { status: 'Approved', decidedBy: 'server.director', decidedAt: '2026-11-01T10:01:00Z' });
    return { status: 200, body: this.invoice };
  } };
  const app = harness(server, 'Director'); await settle();
  assert.match(app.get('pending-list').textContent, /Awaiting approval/);
  assert.doesNotMatch(app.get('pending-list').textContent, /First approved/);
  await app.approve();
  const progress = app.get('pending-list').textContent;
  assert.match(progress, /First approved by server.manager \(Manager\)/);
  assert.match(progress, /Awaiting a distinct Director for second approval/);
  assert.doesNotMatch(progress, /browser.user/);
  assert.equal(server.invoice.status, 'PendingApproval');
  assert.equal(app.get('decision-error').hidden, true);
  const reloaded = harness(server, 'Manager'); await settle();
  assert.equal(reloaded.get('pending-list').textContent, progress);
  assert.ok(reloaded.get('pending-list').find('Approve invoice INV-high'));
  await reloaded.approve();
  assert.equal(reloaded.get('pending-list').textContent, '');
  assert.equal(reloaded.get('pending-empty').hidden, false);
  assert.match(reloaded.get('invoices-table').querySelector('tbody').textContent, /Approved/);
  for (const call of [...app.calls, ...reloaded.calls].filter(call => call.method === 'POST')) {
    assert.equal(call.body, '{}'); assert.equal(call.headers.Authorization, 'Bearer server-token');
    assert.equal(call.url, '/api/approvals/INV-high/approve');
  }
});

for (const [amount, first, reason] of [
  [50000.01, false, 'Manager approval limit is USD 50,000 inclusive; a Director is required.'],
  [500000.01, true, 'A distinct approver is required; the same user cannot approve twice.'],
  [500000.01, true, 'A distinct Director is required for the second approval.']
]) {
  test(`visible decision error survives refresh state for ${reason}`, async () => {
    const invoice = pending(amount);
    if (first) Object.assign(invoice, { firstApprovedBy: 'server.first', firstApprovedRole: 'Director', firstApprovedAt: '2026-11-01T10:00:00Z' });
    const server = { invoice, approve: () => ({ status: 409, body: { error: reason } }) };
    const before = JSON.stringify(invoice); const app = harness(server); await settle();
    const progress = app.get('pending-list').textContent;
    assert.ok(app.get('pending-list').find('Approve invoice INV-high'));
    await app.approve();
    assert.equal(app.get('decision-error').hidden, false); assert.equal(app.get('decision-error').textContent, reason);
    assert.equal(app.get('pending-list').textContent, progress); assert.equal(JSON.stringify(invoice), before);
    const reloaded = harness(server); await settle();
    assert.equal(reloaded.get('pending-list').textContent, progress); assert.equal(JSON.stringify(invoice), before);
    assert.match(reloaded.get('invoices-table').querySelector('tbody').textContent, /Pending Approval/);
  });
}
