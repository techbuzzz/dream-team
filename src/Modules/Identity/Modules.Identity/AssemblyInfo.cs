using DreamTeam.Framework.Web.Modules;
using System.Runtime.CompilerServices;

[assembly: DreamTeamModule(typeof(DreamTeam.Modules.Identity.IdentityModule), 100)]
[assembly: InternalsVisibleTo("Identity.Tests")]