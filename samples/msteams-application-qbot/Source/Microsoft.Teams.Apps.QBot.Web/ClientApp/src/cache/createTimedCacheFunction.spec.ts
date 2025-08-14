/* eslint-disable @typescript-eslint/naming-convention */
import { createTimedCacheFunction } from './createTimedCacheFunction';
import { constant } from 'lodash';

describe('createTimedCacheFunction', () => {
  const now = 100_000_000;
  beforeAll(() => {
    jest.useFakeTimers('modern');
  });
  afterAll(() => {
    jest.useRealTimers();
  });
  describe('arity 0', () => {
    it('should execute the callback once and return the value on first call', () => {
      const callback = jest.fn(() => true);
      const decoratedFn = createTimedCacheFunction(
        10000,
        constant(''),
        callback,
      );
      const result = decoratedFn();
      expect(result).toEqual(true);
      expect(callback).toHaveBeenCalledTimes(1);
    });

    it('should execute the callback once and return the value when called twice', () => {
      const callback = jest.fn(() => true);
      const decoratedFn = createTimedCacheFunction(
        10000,
        constant(''),
        callback,
      );
      const result1 = decoratedFn();
      const result2 = decoratedFn();
      expect(result1).toEqual(true);
      expect(result2).toEqual(true);
      expect(callback).toHaveBeenCalledTimes(1);
    });

    it('should execute the callback once before and after (inclusive) the expiration time has completed', () => {
      const callback = jest.fn(() => true);
      const expiration = 10_000;
      jest.setSystemTime(now);
      const decoratedFn = createTimedCacheFunction(
        expiration,
        constant(''),
        callback,
      );
      const result1 = decoratedFn();
      jest.setSystemTime(now + expiration);
      const result2 = decoratedFn();
      expect(result1).toEqual(true);
      expect(result2).toEqual(true);
      expect(callback).toHaveBeenCalledTimes(2);
    });

    it('should cache the results again after the expiry', () => {
      const callback = jest.fn(() => true);
      const expiration = 10_000;
      jest.setSystemTime(now);
      const decoratedFn = createTimedCacheFunction(
        expiration,
        constant(''),
        callback,
      );
      const result1 = decoratedFn();
      jest.setSystemTime(now + expiration);
      const result2 = decoratedFn();
      const result3 = decoratedFn();
      expect(result1).toEqual(true);
      expect(result2).toEqual(true);
      expect(result3).toEqual(true);
      expect(callback).toHaveBeenCalledTimes(2);
    });
  });

  describe('arity 1', () => {
    it('should execute the callback once per argument value', () => {
      const callback = jest.fn((x: number) => x);
      const decoratedFn = createTimedCacheFunction(10000, (x) => x, callback);
      const result1 = decoratedFn(1);
      const result2 = decoratedFn(2);
      expect(result1).toEqual(1);
      expect(result2).toEqual(2);
      expect(callback).toHaveBeenCalledTimes(2);
    });

    it('should cache the results per argument value', () => {
      const callback = jest.fn((x: number) => x);
      const decoratedFn = createTimedCacheFunction(10000, (x) => x, callback);
      const result1_1 = decoratedFn(1);
      const result2_1 = decoratedFn(2);
      const result1_2 = decoratedFn(1);
      const result2_2 = decoratedFn(2);
      expect(result1_1).toEqual(1);
      expect(result1_2).toEqual(1);
      expect(result2_1).toEqual(2);
      expect(result2_2).toEqual(2);
      expect(callback).toHaveBeenCalledTimes(2);
    });

    it('should clear the cache independently for each argument value', () => {
      const callback = jest.fn((x: number) => x);
      const expiry = 10_000;
      const decoratedFn = createTimedCacheFunction(expiry, (x) => x, callback);
      // call 1 before & after expiry, 2 twice during cache period
      jest.setSystemTime(now);
      const result1_1 = decoratedFn(1);
      jest.setSystemTime(now + 1000);
      const result2_1 = decoratedFn(2);
      jest.setSystemTime(now + expiry);
      const result1_2 = decoratedFn(1);
      const result2_2 = decoratedFn(2);
      expect(result1_1).toEqual(1);
      expect(result1_2).toEqual(1);
      expect(result2_1).toEqual(2);
      expect(result2_2).toEqual(2);
      expect(callback).toHaveBeenCalledTimes(3);
      expect(callback).toHaveBeenNthCalledWith(1, 1);
      expect(callback).toHaveBeenNthCalledWith(2, 2);
      expect(callback).toHaveBeenNthCalledWith(3, 1);
    });
  });
});
