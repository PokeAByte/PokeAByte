using NCalc;

namespace GameHook.Domain
{
    public static class AddressMath
    {
        public static bool TrySolve(string? addressExpression, Dictionary<string, object?> variables, out MemoryAddress address)
        {
            try
            {
                if (string.IsNullOrEmpty(addressExpression))
                {
                    address = 0x00;
                    return false;
                }

                var expression = new Expression(addressExpression);

                foreach (var variable in variables)
                {
                    if (variable.Value == null)
                    {
                        // This is a situation where the javascript has set a variable to null,
                        // this can be done for a variety of reasons that are game-specific.

                        // In this case, we don't want to evalulate the expression, and instead return null.

                        address = 0x00;
                        return false;
                    }

                    expression.Parameters[variable.Key] = variable.Value;
                }

                var result = expression.Evaluate()?.ToString();

                if (uint.TryParse(result, out var castedResult))
                {
                    address = castedResult;
                    return true;
                }
                else
                {
                    address = 0x00;
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Parameter was not defined"))
                {
                    address = 0x00;
                    return false;
                }

                throw;
            }

        }
    }
}
