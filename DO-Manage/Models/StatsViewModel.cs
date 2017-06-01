/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using System.Collections.Generic;
using System.Linq;

namespace DO_Manage.Models
{

    public class StatsViewModel
    {
        public int ContactsOnRemote;
        public int ContactsUpdatedSinceLastSync;
        public int ContactsNotSyncedToO365;
        public int ContactsOnLocal;
    }
}