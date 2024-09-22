using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public static class KoreanNumberProcessor
{
    // https://github.com/Kyubyong/g2pK/blob/master/g2pk/numerals.py

    private static readonly string BOUND_NOUNS = "군데 권 개 그루 닢 대 두 마리 모 모금 뭇 발 발짝 방 번 벌 보루 살 수 술 시 쌈 움큼 정 짝 채 척 첩 축 켤레 톨 통";
    public static bool IsPartOfBoundNoun(char first, char sec)
    {
        return BOUND_NOUNS.Contains($"{first}{sec}");
    }
    public static bool IsBoundNoun(char first)
    {
        return BOUND_NOUNS.Contains(first);
    }

    static string ProcessNum(string num, bool sino = true)
    {
        num = Regex.Replace(num, ",", "");

        if (num == "0")
        {
            return "영";
        }
        if (!sino && num == "20")
        {
            return "스무";
        }

        string digits = "123456789";
        string names = "일이삼사오육칠팔구";
        Dictionary<char, string> digit2name = new Dictionary<char, string>();
        for (int i = 0; i < digits.Length; i++)
        {
            digit2name[digits[i]] = names[i].ToString();
        }

        string modifiers = "한 두 세 네 다섯 여섯 일곱 여덟 아홉";
        string decimals = "열 스물 서른 마흔 쉰 예순 일흔 여든 아흔";
        Dictionary<char, string> digit2mod = new Dictionary<char, string>();
        Dictionary<char, string> digit2dec = new Dictionary<char, string>();
        string[] modArray = modifiers.Split(' ');
        string[] decArray = decimals.Split(' ');

        for (int i = 0; i < digits.Length; i++)
        {
            digit2mod[digits[i]] = modArray[i];
            digit2dec[digits[i]] = decArray[i];
        }

        List<string> spelledout = new List<string>();
        for (int i = 0; i < num.Length; i++)
        {
            int index = num.Length - i - 1;
            char digit = num[i];

            string name = "";
            if (sino)
            {
                if (index == 0)
                {
                    name = digit2name.TryGetValue(digit, out string n) ? n : "";
                }
                else if (index == 1)
                {
                    name = digit2name.TryGetValue(digit, out string n2) ? n2 + "십" : "";
                    name = name.Replace("일십", "십");
                }
            }
            else
            {
                if (index == 0)
                {
                    name = digit2mod.TryGetValue(digit, out string m) ? m : "";
                }
                else if (index == 1)
                {
                    name = digit2dec.TryGetValue(digit, out string d) ? d : "";
                }
            }

            if (digit == '0')
            {
                if (index % 4 == 0)
                {
                    string lastThree = string.Join("", spelledout.GetRange(Math.Max(0, spelledout.Count - 3), Math.Min(3, spelledout.Count)));
                    if (lastThree == "")
                    {
                        spelledout.Add("");
                        continue;
                    }
                }
                else
                {
                    spelledout.Add("");
                    continue;
                }
            }

            if (index == 2)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "백" : "";
                name = name.Replace("일백", "백");
            }
            else if (index == 3)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "천" : "";
                name = name.Replace("일천", "천");
            }
            else if (index == 4)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "만" : "";
                name = name.Replace("일만", "만");
            }
            else if (index == 5)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "십" : "";
                name = name.Replace("일십", "십");
            }
            else if (index == 6)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "백" : "";
                name = name.Replace("일백", "백");
            }
            else if (index == 7)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "천" : "";
                name = name.Replace("일천", "천");
            }
            else if (index == 8)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "억" : "";
            }
            else if (index == 9)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "십" : "";
            }
            else if (index == 10)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "백" : "";
            }
            else if (index == 11)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "천" : "";
            }
            else if (index == 12)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "조" : "";
            }
            else if (index == 13)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "십" : "";
            }
            else if (index == 14)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "백" : "";
            }
            else if (index == 15)
            {
                name = digit2name.TryGetValue(digit, out string n) ? n + "천" : "";
            }
            spelledout.Add(name);
        }

        return string.Join("", spelledout);
    }

    public static string ConvertNum(string input)
    {
        //Log.Debug($"KoreamNumberProcessor input : {input}");
        // Bound Nouns
        char[] inputCharArr = input.ToCharArray();

        var tokens = new HashSet<string>(Regex.Matches(input, @"([\d][\d,]*)([ㄱ-힣]+)/B")
            .Select(m => m.Value));

        //Log.Debug("Tokens: {tokens}", tokens);
       var match = Regex.Match(input, @"([\d][\d,]*)([ㄱ-힣]+)");
        if (match.Success)
        {
            string num = match.Groups[1].Value;
            string bn = match.Groups[2].Value;
            //Log.Debug($"Num: {num}, Bound Noun: {bn}");
            string spelledout = BOUND_NOUNS.Contains(bn) ? ProcessNum(num, false) : ProcessNum(num, true);
            input = input.Replace($"{num}{bn}", $"{spelledout}{bn}");
        }


        // Digit by digit for remaining digits
        string digits = "0123456789";
        string names = "영일이삼사오육칠팔구";
        for (int i = 0; i < digits.Length; i++)
        {
            input = input.Replace(digits[i].ToString(), names[i].ToString());
        }

        return input;
    }


}

