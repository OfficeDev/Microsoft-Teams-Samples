import { createSingleEntrantFunction } from 'cache/createSingleEntrantFunction';
import { User } from 'models';
import { UserService } from 'services/UserService';

/**
 * UserService decorator for single-entrancy on the read operations.
 */
export class UserServiceSingleEntrantDecorator implements UserService {
  constructor(decorated: UserService) {
    this.loadUser = createSingleEntrantFunction(
      (userId: string) => userId,
      decorated.loadUser.bind(decorated),
    );
  }

  loadUser: (userId: string) => Promise<User>;
}
