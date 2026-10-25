# AGENTS.md: Larje UI

Instructions for coding agents working on this package.

## Package layout

- `Runtime/` (`Larje.UI`, namespace `Larje.UI`): `Core/` (UIObject, UIScreen/UIPopup/UIToast and generic `<TView>` variants), `Views/` (UIView, UIStates, UIQuery), `Motion/` (presets and player), `Effects/`, `Bindings/`, `Layout/` (UIKit, UISafeArea), `Factory/`, `Debugging/`, `UIService`, `UIConfig`.
- `Zenject/`: `Larje.UI.Zenject` (`LarjeUIInstaller`, `BindLarjeUI`, `BindUIController<T>(key)`).
- `Editor/`: `Larje.UI.Editor` (`UIViewSync`).
- `Testing/`: `Larje.UI.Testing` for project tests (mockup parity harness, UI Toolkit clicks, panel capture). Compiled only in the editor.
- `Tests/`: EditMode tests.
- This package depends on `com.larje.core` (`~/UnityPackages/unity-gameplay-core-services`). Core must never reference UI.
- The UI targets every platform: no touch-only or portrait-only assumptions.

## Contract with the mockup exporter

`design.py ui-export` (skill `unity-game-designer`, `scripts/uitk.py`) generates what this package consumes. Keep both sides in sync when changing:

- element names = `data-unity`; generated views find elements by name path (`UIQuery.Path`);
- classes `anim--<preset>` (presets in `UIMotionPreset`, names mirrored in `uitk.ANIM_PRESETS`), `state-show--`, `state-hide--`, `state-class--<state>--<class>` (`UIStates`), `asset--<id>`;
- popups: `Backdrop`, `CloseButton`; toasts: `Message`; `.safe` for safe area;
- `UIMockups/manifest.json` and `layout/*.json` format (`MockupLayout`).

## Code style (C#)

- Do not use `var`. Write explicit types.
- Do not write comments in code.
- Do not use `continue`. Restructure the loop or extract a method.
- Seal classes when they are not designed for inheritance.
- Use block bodies `{ }` for methods and `if` statements. Expression-bodied members are fine for simple properties when the surrounding code already uses them.
- Do not name variables `value` except in property setters.
- Make private methods `static` only when a static context requires it.
- Order members as: constants, static fields, serialized fields, fields, properties, events, methods, nested enums, nested classes. Within each group order by access: public, protected, internal, private.
- Private non-serialized fields use `_camelCase`, constants use `UPPER_SNAKE_CASE`. `[SerializeField]` fields follow the style already used in the package (mostly `camelCase`); never rename an existing serialized field without `[FormerlySerializedAs]`, because assets and `com.larje.projectsetup` reference fields by name.
- Serialized fields: never use `[Tooltip]`. Group related fields with `[Header]` and avoid single-field headers.

## Architecture rules

- Services are plain C# classes with constructor injection. Do not add a service locator, singletons or a custom DI container.
- Use UniTask with a `CancellationToken` for loops, delays and async work. Do not use `Update` loops or coroutines in services.
- Ids are ScriptableObject assets. Persistent data lives in save sections owned by the feature.
- Zenject code goes only into the `*.Zenject` assembly. It compiles under `LARJE_ZENJECT`, which comes from `versionDefines` on `com.larje.zenject`.
- Unity AI MCP tools go only into the `*.AI.Editor` assembly. It compiles under `LARJE_UNITY_AI`. Tool names use the `Larje.` prefix and the `larje` group. Put logic that tests need into the regular editor assembly so the tests do not depend on Unity AI.
- Runtime assemblies must not reference editor-only APIs outside `#if UNITY_EDITOR`.

## Working in a Unity project

- Do not edit `.prefab`, `.unity`, `.asset` or `.mat` files as text. Change them through the Unity Editor or MCP tools.
- Commit the `.meta` file for every new file and folder. Git packages are immutable, so missing metas break consumers.
- After every C# change, refresh the editor, wait until compilation is idle, and check that the console has no errors or warnings before moving on.
- Run EditMode tests through the Test Runner. List the package in the project manifest `testables` first.

## Commits

- Write short technical commit messages only. Do not push unless explicitly asked.
