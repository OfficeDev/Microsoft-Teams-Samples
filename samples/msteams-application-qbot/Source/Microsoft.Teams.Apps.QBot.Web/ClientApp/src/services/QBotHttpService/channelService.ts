import { Channel } from 'models';
import { ChannelService } from 'services/ChannelService';
import { Request } from 'services/requestBuilder';

export class HttpChannelService implements ChannelService {
  private readonly request: Request;
  constructor({ request }: { request: Request }) {
    this.request = request;
  }
  loadChannelsForCourse(courseId: string): Promise<Channel[]> {
    return this.request({
      url: `/api/Courses/${courseId}/channels`,
      dtoCast: (channels: Channel[]) => channels,
    });
  }
}
