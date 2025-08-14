import { createSingleEntrantFunction } from 'cache/createSingleEntrantFunction';
import { KnowledgeBaseService } from 'services/KnowledgeBaseService';

export class KnowledgeBaseServiceSingleEntrantDecorator
  implements KnowledgeBaseService {
  constructor(decorated: KnowledgeBaseService) {
    this.loadKnowledgeBases = createSingleEntrantFunction(
      () => 0,
      decorated.loadKnowledgeBases.bind(decorated),
    );
    this.createKnowledgeBase = decorated.createKnowledgeBase.bind(decorated);
    this.updateKnowledgeBase = decorated.updateKnowledgeBase.bind(decorated);
    this.deleteKnowledgeBase = decorated.deleteKnowledgeBase.bind(decorated);
  }

  loadKnowledgeBases: KnowledgeBaseService['loadKnowledgeBases'];
  createKnowledgeBase: KnowledgeBaseService['createKnowledgeBase'];
  updateKnowledgeBase: KnowledgeBaseService['updateKnowledgeBase'];
  deleteKnowledgeBase: KnowledgeBaseService['deleteKnowledgeBase'];
}
