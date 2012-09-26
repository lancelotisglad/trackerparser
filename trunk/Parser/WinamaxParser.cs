using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Awam.Tracker.Data;
using Awam.Tracker.Model;


namespace Awam.Tracker.Parser
{
    public class WinamaxParser : IFileParser
    {
        private static readonly string floatPattern = @"[-+]?[0-9]*\.?[0-9]+";
        private static readonly string actionPattern = @"^(.*) (raises|folds|calls|collected|bets|checks) ?(" + floatPattern + @")?€? ?(to )?([-+]?[0-9]*\.?[0-9]+)?";

        private static readonly Regex _regexParseHeader0 = new Regex("^Winamax Poker - Tournament", RegexOptions.Compiled);
        private static readonly Regex _regexParseHeader = new Regex(@"^Winamax Poker - (.*) - HandId: #([0-9\-]*) - Holdem no limit \((.*)€/(.*)€\) - (.*)", RegexOptions.Compiled);
        private static readonly Regex _regexParseSeats = new Regex(@"^Seat ([0-9]*): (.*) \((.*)€\)", RegexOptions.Compiled);
        private static readonly Regex _regexParseTable = new Regex(@"^Table:(.*)Seat #([1-9]) is the button", RegexOptions.Compiled);
        private static readonly Regex _regexParseAnteBlinds = new Regex(@"^\*\*\* AN", RegexOptions.Compiled);
        private static readonly Regex _regexParsePostsBlind = new Regex(@"^(.*) (posts|denies) (small|big) blind ?(" + floatPattern + @")?€?", RegexOptions.Compiled);
        private static readonly Regex _regexParseDealtToMe = new Regex(@"^Dealt to (.*) \[(.*) (.*)\]", RegexOptions.Compiled);
        private static readonly Regex _regexParsePreFlopTitle = new Regex(@"^\*\*\* PR", RegexOptions.Compiled);
        private static readonly Regex _regexParsePreflop = new Regex(actionPattern, RegexOptions.Compiled);
        private static readonly Regex _regexParseFlopTitle = new Regex(@"^\*\*\* FL", RegexOptions.Compiled);
        private static readonly Regex _regexParseFlop = new Regex(actionPattern, RegexOptions.Compiled);
        private static readonly Regex _regexParseTurnTitle = new Regex(@"^\*\*\* TU", RegexOptions.Compiled);
        private static readonly Regex _regexParseTurn = new Regex(actionPattern, RegexOptions.Compiled);
        private static readonly Regex _regexParseRiverTitle = new Regex(@"^\*\*\* RI", RegexOptions.Compiled);
        private static readonly Regex _regexParseRiver = new Regex(actionPattern, RegexOptions.Compiled);
        private static readonly Regex _regexParseShowDownTitle = new Regex(@"^\*\*\* SH", RegexOptions.Compiled);
        private static readonly Regex _regexParseShowDown1 = new Regex(@"^(.*) (shows) \[(.*) (.*)\]", RegexOptions.Compiled);
        private static readonly Regex _regexParseShowDown2 = new Regex(@"^(.*) collected (" + floatPattern + @")€", RegexOptions.Compiled);
        private static readonly Regex _regexParseSummaryTitle = new Regex(@"^\*\*\* SU", RegexOptions.Compiled);
        private static readonly Regex _regexParseSummary = new Regex(@"^Total pot (.*)€ \| (No rake|Rake (.*)€)", RegexOptions.Compiled);
        private static readonly Regex _regexParseSummary2 = new Regex(@"^Board*|Seat*|^\s*$", RegexOptions.Compiled);

        private string _filePath;
        private DateTime _lastDate;

        delegate bool Step(Hand hand, string line);

        private Step[] ExecuteStep;

        private void log(string line)
        {
            //Console.WriteLine(line);
            //            File.AppendAllLines(@"c:\temp\file.txt", new  []{line});
        }

        public WinamaxParser()
        {   
            ExecuteStep = new Step[] {
                ParseHeader,
                ParseTable,
                ParseSeats,
                ParseAnteBlinds,
                ParsePostsBlind,
                ParseDealtToMe,
                ParsePreFlopTitle,
                ParsePreflop,
                ParseFlopTitle,
                ParseFlop,
                ParseTurnTitle,
                ParseTurn,
                ParseRiverTitle,
                ParseRiver,
                ParseShowDownTitle,
                ParseShowDown,
                ParseSummaryTitle,
                ParseSummary
            };
        }

        public IList<Hand> Parse(string filePath, DateTime lastImport)
        {
            _filePath = filePath;
            _lastDate = lastImport;
            return GetHandsFromFile();
        }

        private IList<Hand> GetHandsFromFile()
        {
            IList<Hand> hands = new List<Hand>();
            using (StreamReader streamReader = 
                new StreamReader(
                    new FileStream(
                        _filePath, 
                        FileMode.Open, 
                        FileAccess.Read, 
                        FileShare.Read),
                    Encoding.UTF8))
            {

                int stepNumber = 0;

                Hand hand = new Hand { Players = new List<HandPlayer>() };

                string line = streamReader.ReadLine();
              
                while (streamReader.Peek() >= 0)
                {
                    bool expressionNotFoundGoToFollowingStepWithTheSameLine = ExecuteStep[stepNumber](hand, line);
                    if (expressionNotFoundGoToFollowingStepWithTheSameLine)
                    {
                        if (stepNumber != ExecuteStep.Count() - 1)
                        {
                            stepNumber++;
                        }
                        else
                        {
                            if (hand.Time > _lastDate)
                            {
                                hands.Add(hand);
                            }
                            hand = new Hand { Players = new List<HandPlayer>() };
                            stepNumber = 0;
                        }
                    }
                    else
                    {
                        line = streamReader.ReadLine();
                    }
                }
                hands.Add(hand);
            }
            return hands;
        }

        private bool ParseHeader(Hand hand, string line)
        {
            
            if (_regexParseHeader0.Match(line).Success)
            {
                throw new FormatException("this is not a cashgame file");
            }

            Match match = _regexParseHeader.Match(line);
            if (match.Success)
            {
                if (match.Groups[1].Value != "CashGame")
                {
                    throw new FormatException("this is not a cashgame file");
                }

                log(line);

                string gameType = match.Groups[1].Value;
                string handId = match.Groups[2].Value;
                string typeDetails = match.Groups[3].Value;
                string bigBlind = match.Groups[4].Value;

                string gameDatetime = match.Groups[5].Value;

                hand.Time = DateTime.ParseExact(
                    gameDatetime,
                    "yyyy/MM/dd HH:mm:ss UTC",
                    CultureInfo.InvariantCulture);


                hand.BigBlind = float.Parse(bigBlind);
                hand.HandId = handId;
                return false;
            }
            return true;
        }

        private bool ParseTable(Hand hand, string line)
        {
            Match match = _regexParseTable.Match(line);
            if (match.Success)
            {
                hand.ButtonPosition = int.Parse(match.Groups[2].Value);
                log(line);
                return false;
            }
            return true;
        }
        
        private bool ParseSeats(Hand hand, string line)
        {
            
            Match match = _regexParseSeats.Match(line);
            if (match.Success)
            {
                log(line);

                hand.Players.Add(
                    new HandPlayer 
                    {
                        Player = match.Groups[2].Value,
                        SeatNumber = int.Parse(match.Groups[1].Value),
                        Stack = decimal.Parse(match.Groups[3].Value)
                    });
                
                string stack = match.Groups[3].Value;
                return false;
            }

            return true;
        }
        
        private bool ParseAnteBlinds(Hand hand, string line)
        {   
            Match match = _regexParseAnteBlinds.Match(line);
            if (match.Success)
            {
                log(line);
                return false;
            }
            return true;
        }

        private bool ParsePostsBlind(Hand hand, string line)
        {
            Match match = _regexParsePostsBlind.Match(line);
            if (match.Success)
            {
                log(line);

                hand[match.Groups[1].Value].ActionBlind = line.Replace(match.Groups[1].Value, "").TrimStart();

                if (match.Groups[4].Value != string.Empty)
                {
                    hand[match.Groups[1].Value].PaidPreflop = decimal.Parse(match.Groups[4].Value);
                }
                return false;
            }
            return true;   
        }
        
        private bool ParseDealtToMe(Hand hand, string line)
        {
           
            Match match = _regexParseDealtToMe.Match(line);
            if (match.Success)
            {
                log(line);

                string card1 = match.Groups[2].Value;
                string card2 = match.Groups[3].Value;

                hand[match.Groups[1].Value].Card1Str = card1;
                hand[match.Groups[1].Value].Card2Str = card2;
                return false;
            }
            return true;
        }

        private bool ParsePreFlopTitle(Hand hand, string line)
        {
            if (_regexParsePreFlopTitle.Match(line).Success)
            {
                log(line);
                return false;
            }

            return true;
        }

        private bool ParsePreflop(Hand hand, string line)
        {
            Match match = _regexParsePreflop.Match(line);

            if (match.Success)
            {
                log(line);
                string player = match.Groups[1].Value;
                string action = match.Groups[2].Value;
                SetAction(match, hand, "preflop");

                return false;
            }
            return true;
        }

        private void SetAction(Match match, Hand hand, string step)
        {
          
            //if (match.Groups[1].Value != me)
            //    return;

            string action = match.Groups[2].Value;

            string actionAndAmount;
            if (action == "raises")
            {
                actionAndAmount = action + " (" + match.Groups[5].Value + ")";
            }
            else
            {
                actionAndAmount = action + " (" + match.Groups[3].Value + ")";
            }


            switch (step)
            {
                case "preflop":
                    hand[match.Groups[1].Value].ActionPreflop +=
                        string.IsNullOrEmpty(hand[match.Groups[1].Value].ActionPreflop) ? actionAndAmount : "," + actionAndAmount;
                    break;
                case "flop":
                    hand[match.Groups[1].Value].ActionFlop +=
                        string.IsNullOrEmpty(hand[match.Groups[1].Value].ActionFlop) ? actionAndAmount : "," + actionAndAmount;
                    break;
                case "turn":
                    hand[match.Groups[1].Value].ActionTurn +=
                        string.IsNullOrEmpty(hand[match.Groups[1].Value].ActionTurn) ? actionAndAmount : "," + actionAndAmount;
                    break;
                case "river":
                    hand[match.Groups[1].Value].ActionRiver +=
                        string.IsNullOrEmpty(hand[match.Groups[1].Value].ActionRiver) ? actionAndAmount : "," + actionAndAmount;
                    break;
            }

           
            if (action == "checks")
                return;

            if (action == "collected")
            {
                hand[match.Groups[1].Value].MyMoneyCollected = decimal.Parse(match.Groups[3].Value);
                return;
            }

            decimal f = 0;
            if (action != "raises")
            {
                if (match.Groups[3].Value != string.Empty)
                    f = decimal.Parse(match.Groups[3].Value);
                switch (step)
                {
                    case "preflop":
                        hand[match.Groups[1].Value].PaidPreflop += f;
                        break;
                    case "flop":
                        hand[match.Groups[1].Value].PaidFlop += f;
                        break;
                    case "turn":
                        hand[match.Groups[1].Value].PaidTurn += f;
                        break;
                    case "river":
                        hand[match.Groups[1].Value].PaidRiver += f;
                        break;
                }
            }
            else
            {
                f = decimal.Parse(match.Groups[5].Value);

                switch (step)
                {
                    case "preflop":
                        hand[match.Groups[1].Value].PaidPreflop = f;
                        break;
                    case "flop":
                        hand[match.Groups[1].Value].PaidFlop = f;
                        break;
                    case "turn":
                        hand[match.Groups[1].Value].PaidTurn = f;
                        break;
                    case "river":
                        hand[match.Groups[1].Value].PaidRiver = f;
                        break;
                }
            }


        }

        private bool ParseFlopTitle(Hand hand, string line)
        {
            Match matchPF = _regexParseFlopTitle.Match(line);
            if (matchPF.Success)
            {
                log(line);
                hand.FlopCard1 = matchPF.Groups[1].Value;
                hand.FlopCard2 = matchPF.Groups[2].Value;
                hand.FlopCard3 = matchPF.Groups[3].Value;

                return false;
            }
            return true;
        }

        private bool ParseFlop(Hand hand, string line)
        {
            Match match = _regexParseFlop.Match(line);
            if (_regexParseFlop.Match(line).Success)
            {
                log(line);
                string player = match.Groups[1].Value;
                string action = match.Groups[2].Value;
                SetAction(match, hand, "flop");
                return false;
            }
            return true;
        }

        private bool ParseTurnTitle(Hand hand, string line)
        {
            Match matchT = _regexParseTurnTitle.Match(line);
            if (matchT.Success)
            {
                log(line);
                hand.TurnCard = matchT.Groups[4].Value;
                return false;
            }
            return true;
        }

        private bool ParseTurn(Hand hand, string line)
        {   
            Match match = _regexParseTurn.Match(line);

            if (match.Success)
            {
                log(line);
                string player = match.Groups[1].Value;
                string action = match.Groups[2].Value;
                     SetAction(match, hand, "turn");
                return false;
            }
            return true;
        }

        private bool ParseRiverTitle(Hand hand, string line)
        {
            
            Match matchR = _regexParseRiverTitle.Match(line);
            if (matchR.Success)
            {
                log(line);
                hand.RiverCard = matchR.Groups[5].Value;
                return false;
            }
            return true;
        }

        private bool ParseRiver(Hand hand, string line)
        {
            Match match = _regexParseRiver.Match(line);

            if (match.Success)
            {
                log(line);
                string player = match.Groups[1].Value;
                string action = match.Groups[2].Value;
                SetAction(match, hand, "river");
                return false;
            }

            return true;
        }

        private bool ParseShowDownTitle(Hand hand, string line)
        {
            
            if (_regexParseShowDownTitle.Match(line).Success)
            {
                log(line);
                return false;
            }
            return true;
        }

        private bool ParseShowDown(Hand hand, string line)
        {
            Match matchSD1 = _regexParseShowDown1.Match(line);
            if (matchSD1.Success)
            {
                hand[matchSD1.Groups[1].Value].Card1Str = matchSD1.Groups[3].Value;
                hand[matchSD1.Groups[1].Value].Card2Str = matchSD1.Groups[4].Value;

                log(line);
                return false;
            }

            Match match = _regexParseShowDown2.Match(line);
            if (match.Success)
            {
                //if (m.Groups[1].Value == me)
                //{
                hand[match.Groups[1].Value].MyMoneyCollected = decimal.Parse(match.Groups[2].Value);
                    log(line);
                //}
                return false;
            }
            return true;
        }

        private bool ParseSummaryTitle(Hand hand, string line)
        {   
            if (_regexParseSummaryTitle.Match(line).Success)
            {
                log(line);
                return false;
            }
            return true;
        }
        
        private bool ParseSummary(Hand hand, string line)
        {
            Match match = _regexParseSummary.Match(line);
            if (match.Success)
            {
                log(line);
                string player = match.Groups[1].Value;
                string action = match.Groups[2].Value;
                return false;
            }

            match = _regexParseSummary2.Match(line);
            if (match.Success)
            {
                log(line);
                string player = match.Groups[1].Value;
                string action = match.Groups[2].Value;
                return false;
            }

            return true;
        }
    }
}
