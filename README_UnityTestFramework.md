# Unity Test Framework для RusRunner

Этот набор файлов добавляет в проект RusRunner базовый каркас автоматических EditMode-тестов на Unity Test Framework/NUnit.

## Что проверяется

Тесты покрывают основную логику проекта, которую можно проверять без запуска игровой сцены:

- `GameContext` — начальное состояние и сброс состояния забега;
- `GameSettings` — сохранение настроек громкости и FPS через `PlayerPrefs`, применение FPS;
- `ProgressionSystem` — базовая скорость, прирост скорости, ограничение сложности;
- `ScoreSystem` — начисление дистанции, очков и сохранение лучшего результата;
- `PowerUpSystem` — применение и завершение бонусов щита, ускорения и множителя очков;
- `AbilitySystem` — регистрация и активация способностей;
- `DashAbility` — временное ускорение и кулдаун;
- `SecondWindAbility` — дополнительный прыжок и кулдаун.


## Как запустить тесты в Unity

1. Откройте проект `RusRunner` через Unity Hub.
2. Дождитесь окончания компиляции скриптов.
3. Откройте окно `Window -> General -> Test Runner`.
4. Перейдите на вкладку `EditMode`.
5. Нажмите `Run All`.

## Запуск из командной строки

Пример для macOS/Linux:

```bash
/Applications/Unity/Hub/Editor/6000.0.43f1/Unity.app/Contents/MacOS/Unity \
  -batchmode \
  -projectPath "RusRunner" \
  -runTests \
  -testPlatform EditMode \
  -testResults "TestResults.xml" \
  -quit
```

Пример для Windows:

```bat
"C:\Program Files\Unity\Hub\Editor\6000.0.43f1\Editor\Unity.exe" ^
  -batchmode ^
  -projectPath "RusRunner" ^
  -runTests ^
  -testPlatform EditMode ^
  -testResults "TestResults.xml" ^
  -quit
```
