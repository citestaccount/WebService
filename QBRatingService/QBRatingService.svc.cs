/* QBRatingService.svc.cs ---------------------------------------------------
 *
 * Informatix Football Apps
 * QB Passer Rating WCF Service
 *
 * © 2013 Jason Barkes - http://jbarkes.blogspot.com
 ----------------------------------------------------------------------------*/

using System;
using System.Net;
using System.Reflection;
using System.ServiceModel.Web;

namespace Barkes.Football
{
    /// <summary>
    /// IQBRatingService interface implementation
    /// </summary>
    public class QBRatingService : IQBRatingService
    {
        public string Version()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            AssemblyName asmName = asm.GetName();
            Version ver = asmName.Version;

            return ver.ToString();
        }

        /// <summary>
        /// Performs the QB rating calculation using the specified formula and formats the
        /// results in the specified format (XML or JSON)
        /// </summary>
        public QBRating GetRating(QBRatingFormula Formula, int Completions, int Attempts,
            int PassingYards, int PassingTDs, int Interceptions, WebMessageFormat ResponseFormat)
        {
            QBRating rating = new QBRating();

            // Set the outgoing response format (XML or JSON)
            WebOperationContext.Current.OutgoingResponse.Format = ResponseFormat;
            
            // Create the appropriate formula (NFL or NCAA)
            switch (Formula)
            {
                case QBRatingFormula.AFL:           // Arena Football League
                case QBRatingFormula.CFL:           // Canadian Football League
                case QBRatingFormula.NFL:           // Nation Football League
                    rating = new NFLQBRating();
                    break;
                case QBRatingFormula.HS:            // High School
                case QBRatingFormula.NCAA:          // College Football
                    rating = new NCAAQBRating();
                    break;
                default:
                    throw new WebFaultException<string>(
                        string.Format("Unsupported formula requested '{0}'", Formula),
                        HttpStatusCode.BadRequest);
            }

            // Set the passing stats
            rating.Completions = Completions;
            rating.Attempts = Attempts;
            rating.Yards = PassingYards;
            rating.TDs = PassingTDs;
            rating.Interceptions = Interceptions;
            
            // Perform the rating calculation
            rating.CalcRating();
            return rating;
        }
    }
}
