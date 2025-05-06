using NCalc;

namespace PokeAByte.Domain;

public static class AddressMath
{
    public static bool TrySolve(Expression? addressExpression, Dictionary<string, object?> variables, out MemoryAddress address)
    {
        if (addressExpression == null)
        {
            address = 0x00;
            return false;
        }
        try
        {
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

                addressExpression.Parameters[variable.Key] = variable.Value;
            }
            var result = addressExpression.Evaluate();
            if (result is double doubleResult)
            {
                address = (uint)doubleResult;
                return true;
            }
            else
            {
                if (uint.TryParse(result?.ToString(), out address))
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
