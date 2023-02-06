import { IApplicationInsights } from '@microsoft/applicationinsights-web';
import { Context } from '@microsoft/teams-js';
import { AuthTokenParameters, ITeamsService } from './TeamsService';

type AsyncFunction = (...args: any[]) => Promise<any>;
type AsyncFunctionReturnType<T extends AsyncFunction> = T extends (
  ...args: any
) => Promise<infer R>
  ? R
  : any;

export class TeamsServiceAIDecorator implements ITeamsService {
  constructor(decorated: ITeamsService, ai: IApplicationInsights) {
    this.getAuthToken = this.trackAsync(
      'getAuthToken',
      ai,
      decorated.getAuthToken.bind(decorated),
    );
    this.getContext = this.trackAsync(
      'getContext',
      ai,
      decorated.getContext.bind(decorated),
    );
    this.registerOnThemeChangeHandler = decorated.registerOnThemeChangeHandler.bind(
      decorated,
    );
  }
  getContext: () => Promise<Context>;
  registerOnThemeChangeHandler: (
    changeHandler: (theme: 'default' | 'dark' | 'contrast') => void,
  ) => Promise<void>;
  getAuthToken: (
    parameters?: AuthTokenParameters | undefined,
  ) => Promise<string>;

  private trackAsync<T extends AsyncFunction>(
    name: string,
    ai: IApplicationInsights,
    fn: T,
  ): (...args: Parameters<T>) => Promise<AsyncFunctionReturnType<T>> {
    return async function (
      ...args: Parameters<T>
    ): Promise<AsyncFunctionReturnType<T>> {
      const startTimeMs = Date.now();
      try {
        const returnValue = await fn(...args);
        ai.trackEvent({
          name,
          properties: {
            success: true,
            duration: startTimeMs - Date.now(),
          },
        });
        return returnValue;
      } catch (ex) {
        ai.trackEvent({
          name,
          properties: {
            success: false,
            duration: startTimeMs - Date.now(),
            error: ex,
          },
        });
        throw ex;
      }
    };
  }
}
