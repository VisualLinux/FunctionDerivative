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

        }
        private MathExpression Parse(string expression)
        {
            List<MathExpression> subExpressions = new List<MathExpression>();
            string newExpression = "";
            int num = 0;
            int lastClosedParenthesis = 0;
            for (int i = 0; i < expression.Length; i++)
            {
                if (expression[i] == '(')
                {
                    subExpressions.Add(Parse(GetFirstParenthesizedString(expression.Substring(i))));
                    newExpression += expression.Substring(lastClosedParenthesis, i + 1);
                    newExpression += "\\" + num;
                    num++;
                }
            }
        }
    }
}