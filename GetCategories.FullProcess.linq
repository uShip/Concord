<Query Kind="Program" />

void Main()
{
	var ASSEMBLY = @"C:\inetpub\wwwroot\uShipDeploy\uShip.RestTests\bin\Debug\uShip.RestTests.dll";
	
	var excludeCategoriesString = @"_Autobid,_BidConversationFeature,_IsReadyForPickup,_ListingBidFeesFeature,_ListingBookItNow,_ListingConversationsFeature,_ListingLocalizations,_ListingPostBookItNow,_ListingsBAN,_ListingsBidFeature,_ListingsHypermedia,_ListingsInternational,_ListingsPaymentCodes,_ListingsSearchFeature,_ListingsStatus,_ListingV1,_SuspendUsers,_TimeFrames,_UsersBids,AnyThread,API_1043,API_433,API_523,API_586,API_643,API_970,CreateAndFunctionBIN,DebugInfoLogs,DeveloperProfiles,Devices,GetUserAddresses,ListingEvents,ListingPostAuction,ListingPostAuctionWithoutTargetOrMaxPrice,ListingPostAuctionWithoutTargetPrice,ListingPostBookItNow,ListingPostEurope,ListingPostFixedPrice,ListingPostWithoutTargetAndMax,ListingsCreate,ListingsFixedPrice,ListingsLister,ListingsNetworkedBrokered,ListingsPublicBrokered,ListingsTrailerTypes,ListingsuShipPaymentsEligibility,ListingsV2,LookupCarriers,LookupsRegions,LookupsServiceTypes,MarketplaceEstimates,MasheryOAuth2SecurityRegistrationAndAccess,OAuth,OAuthApiIntegration,PR_1351?,SavedSearchesFeature,SavedSearchesLegacyFeature,SavedSearchesTestData,SearchFeature,SearchTestDataFeature,TestMessageQueues,TestResources,UserSavedSearchesFeature,UsersCreate,UsersCreateAndFunction,UsersFind,UsersMeWihOAuth,UsersSub,V1Stuff,V2Stuff,Long";
	
	var excludeCategories = excludeCategoriesString.Split(',');
	
	FindTestFixturesExcludingCategories(Assembly.LoadFrom(ASSEMBLY),
								excludeCategories).Dump();;
}


public IList<Type> FindTestFixtures(Assembly assembly)
{
  IEnumerable<Type> types;
  try
  {
      types = assembly.GetTypes();
  }
  catch (ReflectionTypeLoadException loadException)
  {
      types = loadException.Types;
      if (!types.Any())
          throw;
  }
  Func<Type, bool> isTest = t =>
      {
          try
          {
              return t.GetCustomAttributesEndingWith("TestFixtureAttribute").Any();
          }
          catch
          {
              return false;
          }
      };
  return types
      .Where(isTest)
      .ToArray();
}

public IEnumerable<string> FindTestFixturesExcludingCategories(Assembly assembly, IEnumerable<string> excludeCategories)
{
  var testFixtures = FindTestFixtures(assembly);
  return testFixtures.Where(x => !x.HasCategoryAttribute(excludeCategories))
                     .Select(x => x.FullName);
}

public IList<string> FindCategories(Assembly assembly)
{
  var fixtures = FindTestFixtures(assembly);
  return FindCategories(fixtures);
}

public IList<string> FindCategories(IEnumerable<Type> featureTypes)
{
  //return featureTypes
  //    .Select(x => x.GetCustomAttributesEndingWith("CategoryAttribute").FirstOrDefault())
  //    .Where(x => x != null)
  //    .Select(x => x.GetType().GetProperty("Name").GetValue(x).ToString())
  //    .Distinct()
  //    .ToList();
  return featureTypes.GetCategories().ToList();
}


    public static class TypeExtensions
    {
        public static IEnumerable<Attribute> GetCustomAttributesEndingWith(this Type type, string endsWith)
        {
            return type.GetCustomAttributes(true).Dump()
                       .Where(x => x.GetType().Name.EndsWith(endsWith))
                       .Cast<Attribute>();
        }

        public static IEnumerable<string> GetCategories(this IEnumerable<Type> types)
        {
//             return types
//                 .Select(x => x.GetCustomAttributesEndingWith("CategoryAttribute").FirstOrDefault())
//                .Where(x => x != null)
//                .Select(x => x.GetType().GetProperty("Name").GetValue(x).ToString())
//                .Distinct()
//                .ToList();
            return types.Select(GetCategoryAttribute)
                        .Where(x => x != null)
                        .OrderBy(x => x)
                        .Where(x => !"long".Equals(x, StringComparison.OrdinalIgnoreCase))
                        .Distinct();
        }

        private static string GetCategoryName(Attribute a)
        {
            return a.GetType().GetProperty("Name").GetValue(a).ToString();
        }

        private static string GetCategoryAttribute(Type t)
        {
            Func<Attribute, bool> isTheLongRunningAttribute = x => "Long".Equals(GetCategoryName(x),
                                                                                 StringComparison
                                                                                     .OrdinalIgnoreCase);
            var attributes = t.GetCustomAttributesEndingWith("CategoryAttribute").ToArray();
            var firstAttribute = attributes.FirstOrDefault(x => !isTheLongRunningAttribute(x));

            if (firstAttribute == null) return null;

            var firstAttributeName = GetCategoryName(firstAttribute);
            return attributes.Any(isTheLongRunningAttribute)
                       ? "_" + firstAttributeName
                       : firstAttributeName;
        }

        public static bool HasCategoryAttribute(this Type t, IEnumerable<string> categories)
        {
            var categoryAttribute = GetCategoryAttribute(t);
			//new { Name = t.Name, Category = categoryAttribute }.Dump();
            return categoryAttribute != null
                   && categories.Contains(categoryAttribute.TrimLongPrefix(), _categoryComparer);
        }

        private static readonly CategoryNameComparer _categoryComparer = new CategoryNameComparer();
        private class CategoryNameComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return x.TrimLongPrefix().ToLowerInvariant() == y.TrimLongPrefix().ToLowerInvariant();
            }

            public int GetHashCode(string obj)
            {
                return obj.TrimLongPrefix()
                          .ToLowerInvariant()
                          .GetHashCode();
            }
        }

        public static string TrimLongPrefix(this string category)
        {
            return category.TrimStart('_');
        }
    }