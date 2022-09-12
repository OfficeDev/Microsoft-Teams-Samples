import { User } from 'models';

export interface UserService {
  loadUser(userId: string): Promise<User>;
}
