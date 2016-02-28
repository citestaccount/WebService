/* IQBRatingService.cs ------------------------------------------------------
this is a test for Pr Build status *
 * Informatix Football Apps
 * QB Passer Rating WCF Service
 *another comment
 * © 2013 Jason Barkes - http://jbarkes.blogspot.com
 ----------------------------------------------------------------------------*/

using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Barkes.Football
{
    /// <summary>
    /// Available QB passer rating formulas.
    /// </summary>
    [DataContract(Name = "QBRatingFormula")]
    public enum QBRatingFormula
    {
        /// <summary>
        /// National Football League
        /// </summary>
        [EnumMember] NFL,
        /// <summary>
        /// Canadian Football League
        /// </summary>
        [EnumMember] CFL,
        /// <summary>
        /// Arena Football League
        /// </summary>
        [EnumMember] AFL,
        /// <summary>
        /// College Football
        /// </summary>
        [EnumMember] NCAA,
        /// <summary>
        /// High School Football
        /// </summary>
        [EnumMember] HS
    }

    /// <summary>
    /// QB rating base class. Contains passing stats and common calculated stats.
    /// </summary>
    [DataContract(Name = "QBRating")]
    [KnownType(typeof(NFLQBRating)), KnownType(typeof(NCAAQBRating))]
    public class QBRating
    {
        // QB passing stats
        [DataMember] public int Completions { get; set; }
        [DataMember] public int Attempts { get; set; }
        [DataMember] public int Yards { get; set; }
        [DataMember] public int TDs { get; set; }
        [DataMember] public int Interceptions { get; set; }

        // Calculated QB stats (empty setters are required for serialization)
        [DataMember] public double CompletionPercentage
            { get { return Math.Round(((double)Completions / (double)Attempts) * 100, 2); }
              set {} }
        [DataMember] public double YardsPerAttempt
            { get { return Math.Round((double)Yards / (double)Attempts, 2); }
              set {} }
        [DataMember] public double TouchdownPercentage
            { get { return Math.Round(((double)TDs / (double)Attempts) * 100, 2); }
              set {} }
        [DataMember] public double InterceptionPercentage
            { get { return Math.Round(((double)Interceptions / (double)Attempts) * 100, 2); }
              set {} }

        [DataMember] public double Rating { get; set; }

        [OperationContract]
        public virtual double CalcRating()
        {
            return 0.0;
        }
    }

    /// <summary>
    /// NFL QB Rating
    /// </summary>
    [DataContract(Name = "NFLQBRating")]
    public class NFLQBRating : QBRating
    {
        // The NFL formula sets a ceiling for each calculated stat category
        private static double _maxVal = 2.375;

        [OperationContract]
        public override double CalcRating()
        {
            // Completion percentage
            double cpc = Math.Round((CompletionPercentage - 30) * 0.05, 3);
            if (cpc < 0) cpc = 0;
            else if (cpc > _maxVal) cpc = _maxVal;

            // Yards per attempt
            double yac = Math.Round((YardsPerAttempt - 3) * 0.25, 3);
            if (yac < 0) yac = 0;
            else if (yac > _maxVal) yac = _maxVal;

            // TD percentage
            double tpc = Math.Round(TouchdownPercentage * 0.2, 3);
            if (tpc > _maxVal) tpc = _maxVal;

            // Interception percentage
            double ipc = Math.Round(_maxVal - (InterceptionPercentage * 0.25), 3);
            if (ipc < 0) ipc = 0;

            // Final rating
            Rating = Math.Round(((cpc + yac + tpc + ipc) / 6) * 100, 1);

            return Rating;
        }
    }

    /// <summary>
    /// NCAA QB Rating
    /// </summary>
    [DataContract(Name = "NCAAQBRating")]
    public class NCAAQBRating : QBRating
    {
        [OperationContract]
        public override double CalcRating()
        {
            double yac = YardsPerAttempt * 8.4;
            double tpc = TouchdownPercentage * 3.3;
            double ipc = InterceptionPercentage * 2;

            Rating = Math.Round(CompletionPercentage + yac + tpc - ipc, 1);
            return Rating;
        }
    }

    /// <summary>
    /// QB Rating Service interface contract
    /// </summary>
    [ServiceContract(Name = "IQBRatingService")]
    public interface IQBRatingService
    {        
        /// <summary>
        /// Calculates a QB's passer efficiency rating using the specified formula (NFL, NCAA, etc)
        /// REST Example: 10-16, 212 Yds, 3 TD, 1 INT (NFL in XML) 
        /// http://localhost:62370/QBRatingService.svc/calc?formula=NFL&c=10&a=16&y=212&t=3&i=1&fmt=XML
        /// </summary>
        /// <param name="formula">Calculation formula - NFL or NCAA</param>
        /// <param name="completions">Number of passing completions</param>
        /// <param name="attempts">Number of passing attempts</param>
        /// <param name="yards">Number of passing yards</param>
        /// <param name="tds">Number of passing TDs</param>
        /// <param name="interceptions">Number of interceptions</param>
        /// <param name="format">Optional response format specification (XML or JSON)</param>
        /// <returns>QB rating as an XML string</returns>
        [OperationContract, WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Xml,
            UriTemplate = "calc?formula={Formula}&c={Completions}&a={Attempts}&y={PassingYards}&t={PassingTDs}&i={Interceptions}&fmt={ResponseFormat}")]
        QBRating GetRating(QBRatingFormula Formula, int Completions, int Attempts, int PassingYards,
            int PassingTDs, int Interceptions, WebMessageFormat ResponseFormat);

        /// <summary>
        /// Returns the service version.
        /// </summary>
        /// <returns></returns>
        [OperationContract, WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Xml, UriTemplate = "version")]
        string Version();
    }
}
