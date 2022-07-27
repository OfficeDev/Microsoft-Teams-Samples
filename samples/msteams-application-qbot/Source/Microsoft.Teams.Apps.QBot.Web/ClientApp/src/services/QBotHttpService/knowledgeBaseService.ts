import { KnowledgeBase } from 'models';
import { KnowledgeBaseService } from 'services/KnowledgeBaseService';
import { Request } from 'services/requestBuilder';
import { ITeamsService } from 'services/TeamsService';

interface KnowledgeBaseDTO {
  id: string;
  name: string;
  ownerUserId: string;
}

function fromDto(dto: KnowledgeBaseDTO): KnowledgeBase {
  return {
    id: dto.id,
    name: dto.name,
    userId: dto.ownerUserId,
  };
}

function toDto(kb: Partial<KnowledgeBase>): Partial<KnowledgeBaseDTO> {
  return {
    id: kb.id,
    name: kb.name,
    ownerUserId: kb.userId,
  };
}

//TODO(nibeauli): Implement this
export class HttpKnowledgeBaseService implements KnowledgeBaseService {
  private readonly request: Request;
  private readonly teamsService: ITeamsService;
  constructor({
    request,
    teamsService,
  }: {
    request: Request;
    teamsService: ITeamsService;
  }) {
    this.request = request;
    this.teamsService = teamsService;
  }
  async loadKnowledgeBases(): Promise<KnowledgeBase[]> {
    // TODO(nibeauli): The following should probably be done at the middleware level to reduce cross-coupling
    const teamsContext = await this.teamsService.getContext();
    const userId = teamsContext.userObjectId;
    return await this.request({
      url: `/api/users/${userId}/knowledgebases`,
      dtoCast: (dtos: KnowledgeBaseDTO[]) => dtos.map(fromDto),
    });
  }

  async createKnowledgeBase(
    kb: Omit<KnowledgeBase, 'id' | 'userId'>,
  ): Promise<KnowledgeBase> {
    const teamsContext = await this.teamsService.getContext();
    const userId = teamsContext.userObjectId;
    return await this.request({
      method: 'POST',
      url: '/api/knowledgebases',
      body: toDto({ ...kb, userId }),
      dtoCast: fromDto,
    });
  }

  updateKnowledgeBase(kb: KnowledgeBase): Promise<void> {
    return this.request({
      method: 'PUT',
      url: `/api/knowledgebases/${kb.id}`,
      body: toDto(kb),
    });
  }

  deleteKnowledgeBase(id: string): Promise<void> {
    return this.request({
      method: 'DELETE',
      url: `/api/knowledgebases/${id}`,
    });
  }
}
