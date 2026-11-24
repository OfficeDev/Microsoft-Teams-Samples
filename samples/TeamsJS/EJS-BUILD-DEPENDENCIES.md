# EJS in React sample build tools

The React Scripts 4 samples with an `@surma/rollup-plugin-off-main-thread` override
intentionally use EJS **3.1.10**, rather than the plugin's original EJS 2 range.
Use **npm 8.3 or later** so that npm applies these overrides. Keep the override
when regenerating their lockfiles; do not replace it with a direct application
dependency or a React Scripts major-version upgrade.

EJS versions below 3.1.7 are affected by GHSA-phwq-j96m-2c2q, and versions below
3.1.10 are affected by GHSA-ghr5-ch3p-vcr6. Updating only to 3.1.7 is insufficient.
The signing sample already accepts EJS 3, so its lockfile uses a compatible
3.1.10 update without an override. The bulk-meetings sample also accepts EJS 3
after reconciling its lockfile with its existing React Scripts 5 declaration;
it retains the scoped 3.1.10 override.

The plugin renders its bundled AMD loader with `ejs.render(template, options)`.
Its templates use standard interpolation and conditionals, without removed
EJS 2 include syntax. EJS 2.7.4 and 3.1.10 produce identical loader output for
both `useEval` settings and default/custom AMD function names. The pinned
plugin/Rollup combinations (1.4.2/1.32.1 and 2.2.3/2.67.0) were also checked
with shared modules, workers, and dynamic imports. The bulk-meetings
2.2.3/2.80.0 combination was checked with the same execution cases.

Lockfiles retain their existing formats. Regeneration also reconciles existing
manifest/lock drift for the sidepanel HTTP proxy, audio-state/tag Teams SDKs,
and C# incoming-webhook React dependencies. The bulk-meetings lockfile honors
its existing `react-scripts: ^5.0.1` manifest declaration instead of retaining
the stale React Scripts 4 resolution. Its direct webpack 4 development dependency
is unchanged: React Scripts resolves its own isolated webpack 5, and the
production build succeeds with that arrangement. The sidepanel lockfile previously
contained duplicate JSON keys, which npm normalizes during regeneration.

Some legacy samples have pre-existing peer conflicts. When needed, regenerate
and install with `--legacy-peer-deps`; this does not change the EJS override.
Use `--ignore-scripts` for dependency-only validation.

The reconciled bulk-meetings sample was validated with an actual
`npm ci --ignore-scripts --legacy-peer-deps --no-audit --no-fund` installation
and `npm run build`. The production build completes with existing lint warnings
for a Unicode BOM and an array callback without a return value, plus deprecation
and bundle-size warnings.
