// ignore 'any' because this is a lot of type-casting & inferencing.
/* eslint-disable @typescript-eslint/no-explicit-any */
import { Dispatch, Action, AnyAction, Middleware } from 'redux';
import {
  PayloadAction,
  PayloadActionCreator,
  ActionCreatorWithoutPayload,
  createAction,
} from '@reduxjs/toolkit';
import { isString } from 'lodash';

type Handler<
  TCommand extends Action = AnyAction,
  TEvent extends Action = AnyAction
> = (dispatch: Dispatch<TEvent>, command: TCommand) => void;

export type Handlers = Record<string, Handler<PayloadAction<any>, AnyAction>>;

type CommandAction<THandler, TName extends string> = THandler extends (
  dispatch: Dispatch<AnyAction>,
  command: infer Action,
) => void
  ? Action extends { payload: infer P }
    ? PayloadActionCreator<P, TName>
    : ActionCreatorWithoutPayload<TName>
  : ActionCreatorWithoutPayload<TName>;

type CommandActions<THandlers extends Handlers> = {
  [Type in keyof THandlers]: Type extends string
    ? CommandAction<THandlers[Type], Type>
    : never;
};

type CommandsForHandlers<THandlers extends Handlers> = ReturnType<
  CommandActions<THandlers>[keyof THandlers]
>;

interface CreateCommandHandlerOptions<
  THandlers extends Handlers,
  Name extends string = string
> {
  name: Name;
  handlers: THandlers;
}

type CommandHandler<THandlers extends Handlers> = {
  actions: CommandActions<THandlers>;
  middleware: Middleware<CommandsForHandlers<THandlers>>;
};

function getType(handler: string, actionKey: string): string {
  return `${handler}/${actionKey}`;
}

export function createCommandHandler<
  THandlers extends Handlers,
  Name extends string = string
>(
  options: CreateCommandHandlerOptions<THandlers, Name>,
): CommandHandler<THandlers> {
  const { name, handlers } = options;
  const actions: Record<string, Handler> = {};
  const handlerMap: Record<string, Handler<PayloadAction<any>, AnyAction>> = {};
  for (const commandName of Object.keys(options.handlers)) {
    const type = getType(name, commandName);
    handlerMap[type] = handlers[commandName];
    actions[commandName] = createAction(type);
  }
  const middleware: Middleware = (api) => (next) => (action) => {
    const type = action?.type;
    if (isString(type) && type in handlerMap) {
      handlerMap[type](api.dispatch, action);
    }
    next(action);
  };
  return {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    actions: actions as any,
    middleware,
  };
}

export function iterableCommandHandler<
  TCommand extends PayloadAction<any>,
  TEvent extends AnyAction
>(
  handler: (command: TCommand) => Iterable<TEvent> | AsyncIterable<TEvent>,
): Handler<TCommand, TEvent> {
  return async function (dispatch: Dispatch<TEvent>, command: TCommand) {
    for await (const event of handler(command)) {
      dispatch(event);
    }
  };
}

export function asyncCommandHandler<
  TCommand extends PayloadAction<any>,
  TEvent extends AnyAction
>(handler: (command: TCommand) => Promise<TEvent>): Handler<TCommand, TEvent> {
  return (dispatch: Dispatch<TEvent>, command: TCommand) => {
    handler(command).then((event) => dispatch(event));
  };
}

type VoidReturnCommandActions<
  TCommandActions extends CommandActions<Handlers>
> = {
  [key in keyof TCommandActions]: (
    ...args: Parameters<TCommandActions[key]>
  ) => void;
};

export type ActionCreatorMap<
  THandlers extends Record<string, CommandHandler<Handlers>>
> = {
  [actionGroup in keyof THandlers]: VoidReturnCommandActions<
    THandlers[actionGroup]['actions']
  >;
};

type CommandMapForHandlerMap<
  THandlers extends Record<string, CommandHandler<Handlers>>
> = {
  [key in keyof THandlers]: THandlers[key]['middleware'] extends Middleware<
    infer Commands,
    any,
    any
  >
    ? Commands
    : never;
};
export type CommandsForHandlerMap<
  THandlers extends Record<string, CommandHandler<Handlers>>
> = CommandMapForHandlerMap<THandlers>[keyof CommandMapForHandlerMap<THandlers>];

export function mergeCommandHandlers<
  THandlers extends Record<string, CommandHandler<Handlers>>
>(
  handlers: THandlers,
): {
  actionCreators: ActionCreatorMap<THandlers>;
  middleware: Middleware<CommandsForHandlerMap<THandlers>, any, any>;
} {
  // create the action creators
  const actionCreators: Record<string, CommandActions<Handlers>> = {};
  for (const rootName of Object.keys(handlers)) {
    actionCreators[rootName] = handlers[rootName]['actions'];
  }

  const middlewares = Object.values(handlers).map((h) => h.middleware);

  const middleware: Middleware<any, any, any> = (api) => {
    const partialMiddleware = middlewares.map((m) => m(api));
    // compose the middlewares into a single middleware
    return (next) => partialMiddleware.reduce((n, m) => m(n), next);
  };
  // create the merged middleware
  return {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    actionCreators: actionCreators as any,
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    middleware: middleware as any,
  };
}
