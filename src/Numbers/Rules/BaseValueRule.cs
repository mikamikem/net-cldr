﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sepia.Globalization.Numbers.Rules
{
    class BaseValueRule : Rule
    {
        public decimal LowerLimit;
        public decimal UpperLimit = decimal.MaxValue;

        public override void Fire(RbnfContext context)
        {
            foreach (var sub in Substitutions)
            {
                Apply(sub, context);
            }

        }

        void Apply(Substitution sub, RbnfContext context)
        {
            var number = context.Number;
            context.Text.Append(sub.Text);
            switch (sub.Token)
            {
                case "→→":
                    context.Number = Decimal.Remainder(number, Divisor());
                    context.Ruleset.ApplyRules(context);
                    break;
                case "←←":
                    context.Number = Math.Floor(number / Divisor());
                    context.Ruleset.ApplyRules(context);
                    break;
                case "=":
                    // Fallback number formating?
                    if (sub.Descriptor.StartsWith("#") || sub.Descriptor.StartsWith("0"))
                    {
                        context.Text.Append(context.Number.ToString(sub.Descriptor, CultureInfo.InvariantCulture));
                    }

                    // Else goto the ruleset.
                    else
                    {
                        var ruleset = context.RulesetGroup.Rulesets[sub.Descriptor];
                        ruleset.ApplyRules(context);
                    }
                    break;
                case "":
                    break;
                default:
                    throw new NotSupportedException($"Substitution token '{sub.Token}' is not allowed.");
            }

            if (sub.Optional != null && (number % Divisor()) != 0)
            {
                Apply(sub.Optional, context);
            }

            context.Number = number;
        }

        /// <summary>
        ///   The highest power of 10 that is less than or equal to the base value.
        /// </summary>
        decimal Divisor()
        {
            int pow = (LowerLimit == 0) ? 0 : ((int)Math.Floor(Math.Log10((double) LowerLimit)));
            return (decimal) Math.Pow(10, pow);
        }

        public override bool Matches(RbnfContext context)
        {
            return LowerLimit <= context.Number && context.Number <= UpperLimit;
        }

    }
}
