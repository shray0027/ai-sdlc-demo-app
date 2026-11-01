const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const path = require('node:path');
const test = require('node:test');
const vm = require('node:vm');

const source = readFileSync(path.join(__dirname, '../../src/InvoiceApp.Api/wwwroot/app.js'), 'utf8');

function formHarness() {
  const elements = new Map();
  const calls = [];
  const getElementById = (id) => {
    if (!elements.has(id)) elements.set(id, {
      value: '', hidden: false, textContent: '', handlers: {}, resets: 0,
      addEventListener(event, handler) { this.handlers[event] = handler; },
      reset() { this.resets++; },
      setAttribute() {}, append() {}, replaceChildren() {},
      querySelector() { return this; }
    });
    return elements.get(id);
  };
  vm.runInNewContext(source, {
    document: { getElementById, createElement: () => getElementById(Symbol()) },
    sessionStorage: { getItem: () => null, setItem() {}, removeItem() {} },
    fetch: async (url, options) => {
      calls.push({ url, ...options });
      return { status: options.method === 'POST' ? 201 : 200, ok: true, json: async () => [] };
    }
  }, { filename: 'app.js' });
  return {
    calls, elements,
    async submit(amount, customer = 'School') {
      getElementById('amount').value = amount;
      getElementById('customer').value = customer;
      let prevented = false;
      await getElementById('new-invoice-form').handlers.submit({ preventDefault() { prevented = true; } });
      assert.equal(prevented, true);
    }
  };
}

const exactAmounts = [
  '50000.0000000000000000000000001',
  '500000.000000000000000000000001',
  '79228162514264337593543950335.1',
  '50000.000', '0.01', '49999.99', '50000.01', '500000.01',
  '5e4', '1e-2', '500000000000000000000000000001e-24',
  // Policy errors must reach the authoritative server with their exact values.
  '0', '-0.01', '0.001'
];
for (const amount of exactAmounts) {
  test(`form preserves exact numeric token ${amount}`, async () => {
    const harness = formHarness();
    await harness.submit(amount);
    const posts = harness.calls.filter(call => call.method === 'POST');
    assert.equal(posts.length, 1);
    assert.equal(posts[0].url, '/api/invoices');
    assert.equal(posts[0].headers['Content-Type'], 'application/json');
    assert.equal(posts[0].body, `{"customerName":"School","amount":${amount}}`);
    assert.equal(typeof JSON.parse(posts[0].body).amount, 'number');
    assert.equal(harness.elements.get('new-invoice-form').resets, 1);
    assert.equal(harness.elements.get('create-error').hidden, true);
  });
}

for (const [input, expected] of [
  ['+00050000.000', '50000.000'], ['.01', '0.01'], ['-.01', '-0.01'],
  ['000.0100', '0.0100'], ['1.', '1'], ['+001.e+004', '1e+004'],
  ['  +00050.00100e-02  ', '50.00100e-02']
]) {
  test(`form normalizes JSON syntax without changing value: ${input}`, async () => {
    const harness = formHarness();
    await harness.submit(input);
    assert.equal(harness.calls.find(call => call.method === 'POST').body, `{"customerName":"School","amount":${expected}}`);
  });
}

for (const input of [
  '', ' ', '.', '+', 'NaN', 'Infinity', '0x10', '1_000', '1,000', '1e', '1e+',
  '1 2', '1\n2', '"50000"', '1,"role":"Director"', '1}', '1/*comment*/',
  '1e2,"customerName":"injected"', '{"amount":1}', '1;alert(1)'
]) {
  test(`invalid amount cannot inject JSON or submit: ${JSON.stringify(input)}`, async () => {
    const harness = formHarness();
    await harness.submit(input);
    assert.equal(harness.calls.length, 0);
    assert.equal(harness.elements.get('new-invoice-form').resets, 0);
    assert.equal(harness.elements.get('create-error').hidden, false);
    assert.match(harness.elements.get('create-error').textContent, /numeric USD amount/);
  });
}

test('customer text is safely JSON encoded alongside exact amount', async () => {
  const harness = formHarness();
  const customer = 'School "},"amount":0,"role":"Director" \\ \n\t <script> & \u2028';
  const amount = '50000.0000000000000000000000001';
  await harness.submit(amount, customer);
  const body = harness.calls.find(call => call.method === 'POST').body;
  assert.equal(body, `{"customerName":${JSON.stringify(customer)},"amount":${amount}}`);
  const payload = JSON.parse(body);
  assert.equal(payload.customerName, customer);
  assert.deepEqual(Object.keys(payload), ['customerName', 'amount']);
});
