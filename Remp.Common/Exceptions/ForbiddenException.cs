
namespace Remp.Common.Exceptions;

public class ForbiddenException : Exception
{
  public ForbiddenException() : base("You do not have the permission to perform this action.")
  {

  }

  public ForbiddenException(string message) : base(message)
  {
    
  } 
}