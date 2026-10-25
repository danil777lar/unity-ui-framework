# Larje UI

`com.larje.ui`: UI framework for Unity 6 on any platform (mobile, desktop, WebGL). Screens, popups and toasts identified by ScriptableObject ids, a UI service with processors for layering, focus and back navigation, tween-based UI animations, and ready components (currency counters and fly-to-counter, rewarded ad button, banner space, back button, layout helpers). Debug console commands and an optional Zenject installer are included.

Built on [`com.larje.core`](https://github.com/danil777lar/unity-gameplay-core-services) (tweens, input, audio, haptics, economy, monetization, debug console) and UniTask.

## Install

Add to `Packages/manifest.json` (UPM does not resolve git dependencies, so list core and UniTask too):

```json
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.10",
"com.larje.core": "https://github.com/danil777lar/unity-gameplay-core-services.git",
"com.larje.ui": "https://github.com/danil777lar/unity-ui-framework.git"
```

Projects created with `com.larje.projectsetup` get it automatically.

## Setup

1. Create a `UIConfig` (`Create > Larje > UI > UI Config`) and list screen, popup and toast prefabs in it. Set the start screen if one should open on boot.
2. With Zenject: create `LarjeUIInstaller` (`Create > Larje > UI > Zenject > UI Installer`), assign the config, and add it to `ProjectContext` after `LarjeCoreInstaller`. Without Zenject: call `container.BindLarjeUI(config)` from your installer, or construct `UIService` yourself.

## Concepts

| Type | Purpose |
|---|---|
| `UIScreenId`, `UIPopupId`, `UIToastId` | ScriptableObject ids (`Create > Larje > UI > Ids`) |
| `UIScreen`, `UIPopup`, `UIToast` | Root components of prefabs (each root has a `Canvas`); lifecycle Open / Close / Show / Hide / Focus / Unfocus with before/after events |
| `IUIService` | `OpenScreen(id, args)`, `OpenPopup(id, combination, args)`, `ShowToast(id, text, keepOpen)`, `Back()`, `GetOpened()`, focus tracking |
| Processors | `UIScreenProcessor`, `UIPopupProcessor`, `UIToastProcessor` order layers by priority, keep history and handle back; add custom ones with `AddProcessor` |
| `UIObjectArgs` | Data passed to an object on open (`OnBeforeOpen(args)`) |

## Animations

Add `UIAnimation*` components (`Scale`, `Position`, `Rotation`, `CanvasGroup`, `TextColor`, `TextBold`, `Sound`, `Haptic`) to any child of a UI object. Each one plays on the events selected in its list:

- root lifecycle: `RootOpen`, `RootClose`, `RootShow`, `RootHide`, `RootFocus`, `RootUnfocus`. The root waits for the longest open or close animation before finishing.
- pointer and selectable events through `UIEventsRoot` on interactive elements: press, release, hover, click, select, interactable on/off, drag, enable/disable.

Tweens come from `Larje.Core.Tweening` (UniTask, cancellation on disable/destroy).

## Components

- Currency: `CurrencyCounter`, `CurrencyFlyAnimation`, `CurrencyFormatter`.
- Ads: `RewardedAdButton`, `BannerSpace`.
- Common: `UIBackButton`, `LevelCounter`, `ScreenBlocker`, `FirstSelected`, `SliderBlink`, `SpriteAnimation`.
- Layout: `CanvasMatchByAspect`, `FontSizeGroup`, `GridLayoutFitter`.
- `PDWPanel` with its inspector, `VirtualScreen` for UI rendered on world-space surfaces.

Components receive services through `Construct` methods (`[Inject]` under `LARJE_ZENJECT`).

## Debug commands

`UIDebugCommands` (category UI): open screen, open popup, show toast, press back, list opened objects.

## Layout

- `Runtime/`: `Larje.UI`.
- `Zenject/`: `Larje.UI.Zenject` (`LarjeUIInstaller`, `BindLarjeUI`), compiled with `LARJE_ZENJECT`.
- `Editor/`: `Larje.UI.Editor`.
- `Tests/Zenject/`: EditMode tests.
