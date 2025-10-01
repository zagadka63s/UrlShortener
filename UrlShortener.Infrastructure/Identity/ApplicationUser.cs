using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;

namespace UrlShortener.Infrastructure.Identity;

// Расширяем при необходимости (Phone, FullName и т.д.)
public class ApplicationUser : IdentityUser
{
}

