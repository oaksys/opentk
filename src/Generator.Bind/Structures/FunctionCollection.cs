﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Bind.Structures
{
    /// <summary>
    /// Represents a collection of function definitions.
    /// </summary>
    internal class FunctionCollection : SortedDictionary<string, List<FunctionDefinition>>
    {
        private Regex _unsignedFunctions = new Regex(@".+(u[dfisb]v?)", RegexOptions.Compiled);

        private void Add([NotNull] FunctionDefinition f)
        {
            if (!ContainsKey(f.ExtensionName))
            {
                Add(f.ExtensionName, new List<FunctionDefinition>());
                this[f.ExtensionName].Add(f);
            }
            else
            {
                this[f.ExtensionName].Add(f);
            }
        }

        /// <summary>
        /// Adds a range of function definitions to the collection.
        /// </summary>
        /// <param name="functions">The functions.</param>
        public void AddRange([NotNull] IEnumerable<FunctionDefinition> functions)
        {
            foreach (var f in functions)
            {
                AddChecked(f);
            }
        }

        /// <summary>
        /// Adds the function to the collection, if a function with the same name and parameters doesn't already exist.
        /// </summary>
        /// <param name="f">The Function to add.</param>
        private void AddChecked([NotNull] FunctionDefinition f)
        {
            if (ContainsKey(f.ExtensionName))
            {
                var list = this[f.ExtensionName];
                var index = list.IndexOf(f);
                if (index == -1)
                {
                    Add(f);
                }
                else
                {
                    var existing = list[index];
                    var replace = existing.Parameters.Any(p => p.ParameterType.IsUnsigned) &&
                                  !_unsignedFunctions.IsMatch(existing.Name) && _unsignedFunctions.IsMatch(f.Name);
                    replace |= !existing.Parameters.Any(p => p.ParameterType.IsUnsigned) &&
                               _unsignedFunctions.IsMatch(existing.Name) && !_unsignedFunctions.IsMatch(f.Name);
                    replace |=
                        (from oldParameter in existing.Parameters
                            join newParameter in f.Parameters on oldParameter.Name equals newParameter.Name
                            where newParameter.ParameterType.ElementCount == 0 && oldParameter.ParameterType.ElementCount != 0
                            select true)
                        .Count() != 0;
                    if (replace)
                    {
                        list[index] = f;
                    }
                }
            }
            else
            {
                Add(f);
            }
        }
    }
}
