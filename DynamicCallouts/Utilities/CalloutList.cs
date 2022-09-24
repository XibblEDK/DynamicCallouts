using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Attributes;

namespace DynamicCallouts.Utilities
{

    // IDK what to do with this so it'll stay here ig

    internal class CalloutList
    {
        internal static CalloutInfoAttribute[] RegisteredCallouts { get; } = generateCalloutNames().ToArray();
        private static IEnumerable<CalloutInfoAttribute> generateCalloutNames()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (CalloutInfoAttribute calloutInfo in type.GetCustomAttributes<CalloutInfoAttribute>())
                {
                    yield return calloutInfo;
                }
            }
        }
    }
}
