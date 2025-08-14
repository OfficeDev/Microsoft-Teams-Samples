/* eslint-disable sonarjs/no-identical-functions, max-lines-per-function, sonarjs/no-duplicate-string */
import { createSingleEntrantFunction } from './createSingleEntrantFunction';
function createDeferred<TResult>() {
  // eslint-disable-next-line @typescript-eslint/no-empty-function
  let rResolve: (result: TResult) => void = () => {};
  const promise = new Promise<TResult>((resolve) => {
    rResolve = resolve;
  });
  return {
    resolve: rResolve,
    promise,
  };
}

describe('createSingleEntrantFunction - airty zero', () => {
  it('should return the promise from the continuation on first invocation', () => {
    const deferred = createDeferred<string>();
    const singleInvoke = createSingleEntrantFunction(
      () => '',
      () => deferred.promise,
    );
    const retPromise = singleInvoke();
    expect(retPromise).toEqual(deferred.promise);
  });

  it('should only invoke the callback once before the promise resolves when invoked two or more times', () => {
    const deferred = createDeferred<string>();
    let count = 0;
    const singleInvoke = createSingleEntrantFunction(
      () => '',
      () => {
        count = count + 1;
        return deferred.promise;
      },
    );
    const retPromise1 = singleInvoke();
    const retPromise2 = singleInvoke();
    expect(retPromise1).toEqual(deferred.promise);
    expect(retPromise2).toEqual(deferred.promise);
    expect(count).toEqual(1);
  });

  it('should invoke the callback again upon the second request if the first has already resolved', async () => {
    const deferred1 = createDeferred<string>();
    const deferred2 = createDeferred<string>();
    let count = 0;
    const singleInvoke = createSingleEntrantFunction(
      () => '',
      () => {
        count = count + 1;
        if (count <= 1) {
          return deferred1.promise;
        }
        return deferred2.promise;
      },
    );
    const retPromise1 = singleInvoke();
    deferred1.resolve('hello world #1');
    await retPromise1;
    const retPromise2 = singleInvoke();
    deferred2.resolve('hello world #2');
    await retPromise2;
    expect(retPromise1).toEqual(deferred1.promise);
    expect(retPromise2).toEqual(deferred2.promise);
    expect(count).toEqual(2);
  });

  it('should return the same value for concurrent calls', async () => {
    const deferred1 = createDeferred<string>();
    const deferred2 = createDeferred<string>();
    let count = 0;
    // eslint-disable-next-line sonarjs/no-identical-functions
    const singleInvoke = createSingleEntrantFunction(
      () => '',
      () => {
        count = count + 1;
        if (count <= 1) {
          return deferred1.promise;
        }
        return deferred2.promise;
      },
    );
    const retPromise1 = singleInvoke();
    const retPromise2 = singleInvoke();
    deferred1.resolve('hello world #1');
    const ret1 = await retPromise1;
    const ret2 = await retPromise2;

    // invoke a second time after the resolution
    const retPromise3 = singleInvoke();
    deferred2.resolve('hello world #2');
    const ret3 = await retPromise3;

    expect(ret1).toEqual('hello world #1');
    expect(ret2).toEqual('hello world #1');
    expect(ret3).toEqual('hello world #2');
    expect(retPromise1).toEqual(deferred1.promise);
    expect(retPromise3).toEqual(deferred2.promise);
    expect(count).toEqual(2);
  });
});
