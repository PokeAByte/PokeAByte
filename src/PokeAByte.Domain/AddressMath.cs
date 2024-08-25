using NCalc;

namespace PokeAByte.Domain;

public static class AddressMath
{
    public static bool TrySolve(string? addressExpression, Dictionary<string, object?> variables, out MemoryAddress address)
    {
        if (string.IsNullOrEmpty(addressExpression))
        {
            address = 0x00;
            return false;
        }
        try
        {
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

            var result = expression.Evaluate();
            if (result is int castedResult)
            {
                address = (uint)castedResult;
                return true;
            }
            else
            {
                if (uint.TryParse(result.ToString(), out address))
                {
                    return true;
                }
                else
                {
                    address = 0x00;
                    return false;
                }
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
