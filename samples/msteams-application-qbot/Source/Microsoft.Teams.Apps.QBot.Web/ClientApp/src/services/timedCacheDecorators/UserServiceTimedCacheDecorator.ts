import { createTimedCacheFunction } from 'cache/createTimedCacheFunction';
import { User } from 'models';
import { UserService } from 'services/UserService';

/**
 * UserService decorator for single-entrancy on the read operations.
 */
export class UserServiceTimedCacheDecorator implements UserService {
  constructor(decorated: UserService, cacheTimeMs: number) {
    this.loadUser = createTimedCacheFunction(
      cacheTimeMs,
      (userId: string) => userId,
      decorated.loadUser.bind(decorated),
    );
  }

  loadUser: (userId: string) => Promise<User>;
}
