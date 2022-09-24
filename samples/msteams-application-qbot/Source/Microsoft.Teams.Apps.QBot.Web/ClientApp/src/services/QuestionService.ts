import { Question } from 'models';

export interface QuestionService {
  loadQuestions(): Promise<Question[]>;
  updateQuestion(question: Question): Promise<Question>;
  loadAllCourseQuestions(courseId: string): Promise<Question[]>; // Todo: Will be Replaced with proper type.
}
