import { createTimedCacheFunction } from 'cache/createTimedCacheFunction';
import { KnowledgeBaseService } from 'services/KnowledgeBaseService';

export class KnowledgeBaseServiceTimedCacheDecorator
  implements KnowledgeBaseService {
  constructor(decorated: KnowledgeBaseService, cacheTimeMs: number) {
    this.loadKnowledgeBases = createTimedCacheFunction(
      cacheTimeMs,
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
