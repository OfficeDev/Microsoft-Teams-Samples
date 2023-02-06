import { Channel } from 'models';

export interface ChannelService {
  loadChannelsForCourse(courseId: string): Promise<Channel[]>;
}
