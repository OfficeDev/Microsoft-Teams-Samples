import { KnowledgeBase } from 'models';

export interface KnowledgeBaseService {
  loadKnowledgeBases(): Promise<KnowledgeBase[]>;
  createKnowledgeBase(
    kb: Omit<KnowledgeBase, 'id' | 'userId'>,
  ): Promise<KnowledgeBase>;
  updateKnowledgeBase(kb: KnowledgeBase): Promise<void>;
  deleteKnowledgeBase(id: string): Promise<void>;
}
