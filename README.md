# Larje UI

`com.larje.ui`: UI framework on **UI Toolkit** for Unity 6 on any platform (mobile, desktop, WebGL). Screens, popups and toasts are UXML/USS documents exported from HTML/CSS design mockups; behaviour lives in plain C# controllers with constructor injection; animations are presets declared in the markup; layout parity with the mockups is tested automatically.

Built on [`com.larje.core`](https://github.com/danil777lar/unity-gameplay-core-services) (tweens, input, economy) and UniTask.

## Install

```json
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.10",
"com.larje.core": "https://github.com/danil777lar/unity-gameplay-core-services.git",
"com.larje.ui": "https://github.com/danil777lar/unity-ui-framework.git"
```

Projects created with `com.larje.projectsetup` get it configured: `PanelSettings` + theme, `UIConfig`, `LarjeUIInstaller` in `ProjectContext`.

## Pipeline

1. Design: mockups in the design folder (`unity-game-designer` skill): HTML pages built from the portable kit (`kit.css`), tokens from `style.json`.
2. `design.py ui-export <design> <project>` writes `Assets/_Project/UI` (UXML per screen/popup/toast, `tokens.uss`, `kit.uss`, `game.uss`, `assets.uss`, page USS, fonts, art), generated views `Assets/_Project/Scripts/UI/Views/<Name>View.cs` + `UIKeys.cs`, and `UIMockups/` test data, then runs `Larje.UI.Editor.UIViewSync` (creates ids, fills `UIConfig`).
3. Controllers add behaviour; `UIMockupTests` (PlayMode) compares every exported view and state with the mockup layout and writes side-by-side images to `Logs/UIParity`.

Never edit exported UXML/USS/views by hand: change the mockup and export again.

## Runtime

| Type | Purpose |
|---|---|
| `UIConfig` | `PanelSettings`, view entries (id → `VisualTreeAsset`) for screens, popups, toasts, start screen, motion speed, toast duration, sync folders |
| `IUIService` | `OpenScreen`, `OpenPopup` (`Close` / `Overlay` / `Hide` combination), `ShowToast(text, keepOpen)`, `Close`, `Back`, focus, `Opened`/`Closed` events, `Root` and `OverlayLayer` |
| `UIScreen<TView>`, `UIPopup<TView>`, `UIToast<TView>` | Controllers: override `OnViewReady`, `OnOpen(args)`, `OnOpened`, `OnClose`, `OnFocus`, `OnBack`; `View` is the generated typed view, `States` toggles mockup states, `Lifetime` is cancelled on close |
| `UIView` | Base of generated views: `Find<T>("Path/To/Name")` |
| `UIStates` | Mockup states (`state-show--x`, `state-hide--x`, `state-class--x--cls` classes): `Set("no_ads", true)` |
| `UIMotion`, `UIMotionPreset` | Presets from `anim--*` classes: appear (`fade-in`, `popup-in`, `pop-in`, `toast-in`, `slide-up/down/left/right-in`, reversed on close), `press`, `hover-grow`, idle (`idle-pulse`, `idle-wiggle`, `idle-float`, `idle-spin`) |
| `UIFlyEffect` | Coins/items flying from one element to another with a burst, curve, stagger and target punch |
| `UICurrencyLabel` | Label bound to a currency with count-up animation |
| `UISafeArea` | `.safe` elements get max(device safe area, design insets) |
| `UIKit` | Prepares instantiated layouts (stretch, strips default theme classes from `Button`/`Label` so the kit styles win) |
| `IUIControllerFactory` | Key → controller (`UIControllerRegistry` without Zenject, `ZenjectUIControllerFactory` with it) |

Popups close on `Backdrop` click and `CloseButton`; toasts close on tap or after the duration (`keepOpen` keeps them).

## Zenject

```csharp
Container.BindUIController<MainMenuScreen>(UIKeys.MainMenu);
Container.BindUIController<ShopPopup>(UIKeys.ShopPopup);
```

`LarjeUIInstaller` (in `ProjectContext` after `LarjeCoreInstaller`) calls `BindLarjeUI(config)`.

```csharp
public sealed class ShopPopup : UIPopup<ShopPopupView>
{
    private readonly IAdsService _ads;

    public ShopPopup(IAdsService ads)
    {
        _ads = ads;
    }

    protected override void OnViewReady()
    {
        View.WatchAdButton.clicked += OnWatchAd;
        States.Set("no_ads", _ads.NoAds);
    }

    private void OnWatchAd()
    {
        _ads.ShowRewardedAsync("shop_coins", Lifetime).Forget();
    }
}
```

## Testing (`Larje.UI.Testing`, editor only)

- `UIMockupSuite`: runs all exported mockups and states from `UIMockups/manifest.json`; `UIMockupHarness` renders UXML into a texture at the reference resolution, compares element rects with the mockup layout (tolerance from the manifest) and saves `Logs/UIParity/<name>.png` and `<name>--compare.png` (mockup | Unity).
- `UIInteraction`: find and click UI Toolkit buttons by name or text.
- `UIPanelCapture`: renders all UI panels into a texture (used by `PlaytestSession.CaptureAsync`).

## Debug commands

Category UI: open screen / popup / toast (by id or key), press back, list opened.

## Layout

- `Runtime/`: `Larje.UI`.
- `Zenject/`: `Larje.UI.Zenject` (`LARJE_ZENJECT`).
- `Editor/`: `Larje.UI.Editor` (`UIViewSync`, menu `Tools/Larje/UI/Sync Views`).
- `Testing/`: `Larje.UI.Testing` (`UNITY_EDITOR`).
- `Tests/`: EditMode tests.
