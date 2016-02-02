/*  Copyright 2015 Mark McAvoy
 * 
 *  This file is part of ShoutboxSPA.
 *
 *  ShoutboxSPA is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  ShoutboxSPA is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with ShoutboxSPA.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */

using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoutboxSpa.Services
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("ShoutboxSpa", "default", "{controller}/{action}", new[] { "ShoutboxSpa.Services" });
        }
    }
}