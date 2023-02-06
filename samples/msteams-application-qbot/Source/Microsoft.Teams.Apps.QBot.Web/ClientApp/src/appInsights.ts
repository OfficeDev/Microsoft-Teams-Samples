import {
  ReactPlugin,
  withAITracking as wAITracking,
} from '@microsoft/applicationinsights-react-js';
import React from 'react';

export const reactPlugin = new ReactPlugin();
export function trackPageComponent<ComponentType>(
  Component: React.ComponentType<ComponentType>,
): React.ComponentType<ComponentType> {
  return wAITracking(reactPlugin, Component, undefined, 'page-component');
}
