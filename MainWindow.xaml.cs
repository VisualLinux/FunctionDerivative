using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using CalculateDerivative;
using CalculateDerivative.Operations;

namespace Derivative
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            if (Function.Text.IndexOf('\\') != -1)
            {
                IsValidText.Text = false.ToString();
            }
            IsValidText.Text = IsValid(Function.Text).ToString();
        }
        private bool IsValid(string expression)
        {
            List<char> validCharacters = new List<char>();
            for (char i = 'A'; i <= 'z'; i++)
            {
                validCharacters.Add(i);
            }
            for (char i = '0'; i <= '9'; i++)
            {
                validCharacters.Add(i);
            }
            validCharacters.Add('+');
            validCharacters.Add('-');
            validCharacters.Add('*');
            validCharacters.Add('/');
            validCharacters.Add('^');
            validCharacters.Add('(');
            validCharacters.Add(')');
            validCharacters.Add('\\');
            if (expression.Distinct().Except(validCharacters).Count() > 0)
            {
                return false;
            }
            if (expression.Intersect(new[] { '(' }).Count() != expression.Intersect(new[] { ')' }).Count())
            {
                return false;
            }
            List<char> atoms = validCharacters.TakeWhile(x => x != '+').ToList();
            List<char> operators = validCharacters.SkipWhile(x => x != '9').ToList();
            List<string> function = new List<string>() { "log" };
            string newExpression = "";
            List<string> expressions = expression.IndexOf('(') == -1 ? new List<string>() : GetParenthesizedStrings(expression, ref newExpression);
            if (expressions.Count == 0)
            {
                newExpression = expression;
            }
            for (int i = 0; i < expressions.Count; i++)
            {
                string expr = expressions[i];
                if (!IsValid(expr))
                {
                    return false;
                }

            }
            Regex log = new Regex("log\\\\(\\d+)");
            newExpression = log.Replace(newExpression, x => "\\" + x.Groups[1].Value);
            Regex regex = new Regex("((\\d+|[a-zA-Z]+|\\\\\\d+)(\\+|-|\\*|/|\\^))*(\\d+|[a-zA-Z]|\\\\\\d+)");
            return regex.Match(newExpression).Value == newExpression;
        }
        private List<string> GetParenthesizedStrings(string expression, ref string newExpression)
        {
            List<string> strings = new List<string>();
            int index = 0;
            int num = 0;
            newExpression += expression.Substring(index, expression.Substring(index).IndexOf('(')) + "\\" + num;
            num++;
            index += expression.Substring(index).IndexOf('(');
            strings.Add(GetFirstParenthesizedString(expression.Substring(index)));
            index += strings[strings.Count - 1].Length + 2;
            while (index != expression.Length)
            {
                if (expression.Substring(index).IndexOf('(') == -1)
                {
                    break;
                }
                newExpression += expression.Substring(index, expression.Substring(index).IndexOf('(')) + "\\" + num;
                num++;
                index += expression.Substring(index).IndexOf('(');
                strings.Add(GetFirstParenthesizedString(expression.Substring(index)));
                index += strings[strings.Count - 1].Length + 2;
            }
            return strings;
        }
        private static string GetFirstParenthesizedString(string expression)
        {
            int startIndex = 0;
            int endIndex = 0;
            int open = 0;
            int close = 0;
            startIndex = expression.IndexOf('(') + 1;
            if (startIndex == 0)
            {
                return "";
            }
            open++;
            int index = startIndex - 1;
            while (open != close)
            {
                index++;
                switch (expression[index])
                {
                    case '(':
                        open++;
                        break;
                    case ')':
                        close++;
                        break;
                    default:
                        break;
                }
            }
            endIndex = index - 1;
            return (expression.Substring(startIndex, endIndex - startIndex + 1));
        }

        private void Derivate_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValid(Function.Text))
            {
                IsValidText.Text = false.ToString();
                return;
            }
            MathExpression function = Parse(Function.Text);
            MathExpression derivative = function.Root.Derivative(Variable.Text);
            Derivative.Text = derivative.ToString();
        }
        private MathExpression Parse(string expression)
        {
            List<Tuple<string, MathExpression>> mathExpressionTuple = Lexer(expression);
            List<MathExpression> mathExpression = mathExpressionTuple.ConvertAll(x => x.Item2);
            for (int i = mathExpression.Count - 1; i >= 0; i--)
            {
                if (mathExpressionTuple[i].Item1 == "^")
                {
                    mathExpression[i] = new MathExpression(new Power(new[] { mathExpression[i - 1].Root, mathExpression[i + 1].Root }));
                    mathExpression.RemoveAt(i - 1);
                    mathExpression.RemoveAt(i);
                    i = mathExpression.Count;
                }
            }
            for (int i = 0; i < mathExpression.Count; i++)
            {
                if (mathExpressionTuple[i].Item1 == "*")
                {
                    mathExpression[i] = new MathExpression(new Multiply(new[] { mathExpression[i - 1].Root, mathExpression[i + 1].Root }));
                    mathExpression.RemoveAt(i - 1);
                    mathExpression.RemoveAt(i);
                    i = -1;
                }
                else if (mathExpressionTuple[i].Item1 == "/")
                {
                    mathExpression[i] = new MathExpression(new Divide(new[] { mathExpression[i - 1].Root, mathExpression[i + 1].Root }));
                    mathExpression.RemoveAt(i - 1);
                    mathExpression.RemoveAt(i);
                    i = -1;
                }
            }
            for (int i = 0; i < mathExpression.Count; i++)
            {
                if (mathExpressionTuple[i].Item1 == "+")
                {
                    mathExpression[i] = new MathExpression(new Add(new[] { mathExpression[i - 1].Root, mathExpression[i + 1].Root }));
                    mathExpression.RemoveAt(i - 1);
                    mathExpression.RemoveAt(i);
                    i = -1;
                }
                else if (mathExpressionTuple[i].Item1 == "-")
                {
                    mathExpression[i] = new MathExpression(new Subtract(new[] { mathExpression[i - 1].Root, mathExpression[i + 1].Root }));
                    mathExpression.RemoveAt(i - 1);
                    mathExpression.RemoveAt(i);
                    i = -1;
                }
            }
            return mathExpression[0];
        }
        private List<Tuple<string, MathExpression>> Lexer(string expression)
        {
            List<Tuple<string, MathExpression>> Tokens = new List<Tuple<string, MathExpression>>();
            List<MathExpression> subExpressions = new List<MathExpression>();
            string newExpression = "";
            int num = 0;
            int lastClosedParenthesis = -1;
            for (int i = 0; i < expression.Length; i++)
            {
                if (expression[i] == '(')
                {
                    subExpressions.Add(Parse(GetFirstParenthesizedString(expression.Substring(i))));
                    newExpression += expression.Substring(lastClosedParenthesis + 1, i - lastClosedParenthesis - 1);
                    newExpression += "\\" + num;
                    num++;
                }
                if (expression[i]==')')
                {
                    lastClosedParenthesis = i;
                }
            }
            if (expression.IndexOf('(') == -1)
            {
                newExpression = expression;
            }
            Regex log = new Regex("log\\\\(\\d+)");
            newExpression = log.Replace(newExpression, x =>
            {
                subExpressions[int.Parse(x.Groups[1].Value)] = new MathExpression(
                            new Log(new[] { subExpressions[int.Parse(x.Groups[1].Value)].Root }, Math.E));
                return "\\" + x.Groups[1].Value;
            });
            for (int i = 0; i < newExpression.Length;)
            {
                if (int.TryParse(newExpression[i].ToString(), out _))
                {
                    int j = i;
                    while (int.TryParse(newExpression[j].ToString(), out _))
                    {
                        j++;
                        if (j == newExpression.Length)
                        {
                            break;
                        }
                    }
                    Tokens.Add(new Tuple<string, MathExpression>("num", new MathExpression(new Constant(int.Parse(newExpression.Substring(i, j - i))))));
                    i = j;
                }
                else if (newExpression[i] == '\\')
                {
                    int j = i + 1;
                    while (int.TryParse(newExpression[j].ToString(), out _))
                    {

                        j++;
                        if (j == newExpression.Length)
                        {
                            break;
                        }
                    }
                    Tokens.Add(new Tuple<string, MathExpression>("expr", subExpressions[int.Parse(newExpression.Substring(i + 1, j - i - 1))]));
                    i = j;
                }
                else if (new[] { '+', '-', '*', '/', '^' }.Intersect(new[] { newExpression[i] }).Count() == 0)
                {
                    int j = i;
                    while (new[] { '+', '-', '*', '/', '^' }.Intersect(new[] { newExpression[j] }).Count() == 0)
                    {
                        j++;
                        if (j == newExpression.Length)
                        {
                            break;
                        }
                    }
                    Tokens.Add(new Tuple<string, MathExpression>("var", new MathExpression(new Variable(newExpression.Substring(i, j - i)))));
                    i = j;
                }
                else
                {
                    switch (newExpression[i])
                    {
                        case '+':
                            Tokens.Add(new Tuple<string, MathExpression>("+", null));
                            break;
                        case '-':
                            Tokens.Add(new Tuple<string, MathExpression>("-", null));
                            break;
                        case '*':
                            Tokens.Add(new Tuple<string, MathExpression>("*", null));
                            break;
                        case '/':
                            Tokens.Add(new Tuple<string, MathExpression>("/", null));
                            break;
                        case '^':
                            Tokens.Add(new Tuple<string, MathExpression>("^", null));
                            break;
                        default:
                            break;
                    }
                    i++;
                }
            }
            return Tokens;
        }
    }
}
