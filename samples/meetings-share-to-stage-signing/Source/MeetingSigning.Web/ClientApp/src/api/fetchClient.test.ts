import { createHeaders } from './fetchClient';

const fakeToken = 'auth-token';

test('createHeaders with token and header, returns correct headers', () => {
  const testHeaderValue = 'TestValue';

  const initHeaders: HeadersInit = { TestHeader: testHeaderValue };
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const headers: any = createHeaders(fakeToken, initHeaders);

  expect(headers['Authorization']).toBe(`Bearer ${fakeToken}`);
  expect(headers['TestHeader']).toBe(testHeaderValue);
});

test('createHeaders with token and undefined headers, returns correct headers', () => {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const headers: any = createHeaders(fakeToken);

  expect(headers['Authorization']).toBe(`Bearer ${fakeToken}`);
});

test('createHeaders with token and empty header, returns correct headers', () => {
  const initHeaders: HeadersInit = {};
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const headers: any = createHeaders(fakeToken, initHeaders);

  expect(headers['Authorization']).toBe(`Bearer ${fakeToken}`);
});
