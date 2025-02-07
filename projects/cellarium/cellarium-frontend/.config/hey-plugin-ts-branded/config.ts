import type { Plugin } from '@hey-api/openapi-ts';

import { handler } from './plugin';
import type { Config } from './types';

export const defaultConfig: Plugin.Config<Config> = {
    _dependencies: [],
    _handler: handler,
    _handlerLegacy: () => {},
    myOption: false, // implements default value from types
    name: 'hey-plugin-ts-branded',
    output: 'my-plugin',
};

/**
 * Type helper for `my-plugin` plugin, returns {@link Plugin.Config} object
 */
export const defineConfig: Plugin.DefineConfig<Config> = (config) => ({
    ...defaultConfig,
    ...config,
});