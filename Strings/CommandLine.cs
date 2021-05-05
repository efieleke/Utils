using System.Collections.Generic;

namespace Sayer.Strings
{
    public static class CommandLine
    {
        /// <summary>
        /// Given a collection of arguments, returns a mapping of argument name to value.
        /// </summary>
        /// <param name="args">
        /// Expected to be in a format like this: ["/SomeFlag", "/SomeProperty=foobar"]
        /// The forward slash is optional. It may be omitted or replaced with a hyphen.
        /// Each arg represents an option that may or may not have a value associated with it (depending
        /// upon whether there is an equals sign).
        /// Everything before the first '=' is the name of the option, and everything after is the value.
        /// If the the argument doesn't have an '=' (or ends with '='), then the whole arg is the name
        /// of the option and its value is null.
        /// </param>
        /// <returns>A mapping of property name to value (properties with no values have a null for the associated value)</returns>
        public static Dictionary<string, string> Parse(string[] args)
        {
            var options = new Dictionary<string, string>();

            if (args != null)
            {
                foreach (string arg in args)
                {
                    if (arg.Length > 0)
                    {
                        string option = arg[0] == '/' || arg[0] == '-' ? arg.Substring(1) : arg;
                        int index = option.IndexOf('=');

                        if (index > 0)
                        {
                            options.Add(
                                option.Substring(0, index),
                                index == option.Length - 1 ? null : option.Substring(index + 1));
                        }
                        else
                        {
                            options.Add(option, null);
                        }
                    }
                }
            }

            return options;
        }
    }
}
