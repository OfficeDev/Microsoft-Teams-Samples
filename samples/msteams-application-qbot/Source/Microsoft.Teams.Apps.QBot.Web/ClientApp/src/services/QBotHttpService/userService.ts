import { User } from 'models';
import { UserService } from 'services/UserService';
import { Request } from 'services/requestBuilder';

interface UserDTO {
  aadId: string;
  name: string;
  profilePicUrl?: string;
  teamId?: string;
  upn: string;
}
function castDto(dto: UserDTO): User {
  return {
    id: dto.aadId,
    aadId: dto.aadId,
    name: dto.name,
    iconUrl: dto.profilePicUrl
      ? `data:image/png;base64, ${dto.profilePicUrl}`
      : undefined,
    upn: dto.upn,
  };
}

export class HttpUserService implements UserService {
  private readonly request: Request;
  constructor({ request }: { request: Request }) {
    this.request = request;
  }
  async loadUser(userId: string): Promise<User> {
    return this.request({
      url: `/api/Users/${userId}`,
      dtoCast: (user: UserDTO) => castDto(user),
    });
  }
}
