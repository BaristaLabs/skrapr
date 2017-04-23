namespace BaristaLabs.Skrapr.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Input = ChromeDevTools.Input;

    public static class InputUtils
    {
        private static Regex m_commandRegex = new Regex(@"(?<Modifier>[!+^#]?)(?:(?<Letter>[^{}])|(?:{(?<Command>.*?)}))", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        public static IEnumerable<Input.DispatchKeyEventCommand> ConvertInputToKeyEvents(string input)
        {
            foreach (Match result in m_commandRegex.Matches(input))
            {
                if (result.Groups["Letter"].Success)
                    yield return MapLetterToKeyEvent(result.Groups["Modifier"].Value, result.Groups["Letter"].Value);
                else
                    yield return MapCommandToKeyEvent(result.Groups["Modifier"].Value, result.Groups["Command"].Value);
            }
        }

        private static Input.DispatchKeyEventCommand MapLetterToKeyEvent(string modifier, string letter)
        {
            ConsoleKey keyCode;
            switch(letter)
            {
                case " ":
                    keyCode = ConsoleKey.Spacebar;
                    break;
                case ",":
                    keyCode = ConsoleKey.OemComma;
                    break;
                default:
                    keyCode = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), letter.ToUpperInvariant());
                    break;
            }

            var result = new Input.DispatchKeyEventCommand
            {
                Modifiers = GetModifier(modifier),
                Text = letter,
                //Key = letter,
                NativeVirtualKeyCode = (long)keyCode,
                WindowsVirtualKeyCode = (long)keyCode,
                Type = "keyDown",
                Timestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds(),
            };

            if (result.Modifiers.HasValue && result.Modifiers.Value == 8)
                result.Text = result.Text.ToUpperInvariant();

            return result;
        }

        private static Input.DispatchKeyEventCommand MapCommandToKeyEvent(string modifier, string command)
        {
            var result = new Input.DispatchKeyEventCommand
            {
                Modifiers = GetModifier(modifier),
                Type = "keyDown",
                Timestamp = DateTimeOffset.Now.ToUniversalTime().ToUnixTimeSeconds()
            };

            switch (command)
            {
                case "Enter":
                    result.Type = "rawKeyDown";
                    result.NativeVirtualKeyCode = 13;
                    result.WindowsVirtualKeyCode = 13;
                    break;
                case "!":
                case "+":
                case "^":
                case "#":
                case "{":
                case "}":
                    result.Text = command;
                    break;
            }

            

            return result;
        }

        private static long? GetModifier(string modifier)
        {
            if (String.IsNullOrWhiteSpace(modifier))
                return null;

            switch (modifier.ToUpperInvariant())
            {
                case "!": //Alt
                    return 1;
                case "^": //Ctrl
                    return 2;
                case "#": //Meta/Command/Win/Yada
                    return 4;
                case "+": //Shift
                    return 8;
                default:
                    return 0;
            }
        }
    }
}
