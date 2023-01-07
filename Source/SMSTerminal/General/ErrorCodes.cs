using NLog;

namespace SMSTerminal.General
{
    public sealed class ModemErrorMessage
    {
        public ModemErrorMessage(ErrorType errorType, int code, string message)
        {
            ErrorType = errorType;
            Number = code;
            Message = message;
        }
        public int Number { get; }

        public string Message { get; }

        public ErrorType ErrorType { get; }
    }

    internal static class ErrorCodes
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly List<ModemErrorMessage> ErrorMessageList = new();
        private static readonly object LockHasError = new();

        /// <summary>
        /// Checks for CME and CMS Error and returns verbose error message if only error code was given.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Tuple<bool, string> HasCError(string data)
        {
            lock (LockHasError)
            {
                if (string.IsNullOrEmpty(data))
                {
                    return new Tuple<bool, string>(false, "");
                }

                var containsCError = data.Contains(ATMarkers.CMSErrorKeyword) || data.Contains(ATMarkers.CMEErrorKeyword);
                var result = new Tuple<int, string>(0, "");

                if (data.Contains(ATMarkers.CMSErrorKeyword))
                {
                    result = GetMessage(ErrorType.CMS, data);
                }
                if (data.Contains(ATMarkers.CMEErrorKeyword))
                {
                    result = GetMessage(ErrorType.CME, data);
                }
                return new Tuple<bool, string>(containsCError, result.Item2);
            }
        }

        private static Tuple<int, string> GetMessage(ErrorType errorType, string data)
        {
            try
            {
                /*
                 * Some modems returns verbose error messages after setting AT+CMEE, others don't..
                 * AT+CSMS=1;+CNMI=3,3,0,2,1\r\n\r\n+CMS ERROR: 500\r\n
                 * AT+CSMS=1;+CNMI=3,3,0,2,1\r\n\r\n+CMS ERROR: Unknown Error\r\n
                 */
                if (string.IsNullOrEmpty(data))
                {
                    return new Tuple<int, string>(-999, "");
                }
                
                var errorIdentifier = errorType == ErrorType.CME ? "+CME" : "+CMS";
                var messageRows = data.Split('\r', StringSplitOptions.RemoveEmptyEntries).ToList();
                messageRows.ForEach(o => o = o.Trim());

                var errorMessage = "";

                foreach (var s in messageRows.Where(s => s.Contains("ERROR:") && s.Contains(errorIdentifier)))
                {
                    /*
                     * This is the row we are  after
                     */
                    errorMessage = s.Trim();
                    break;
                }
                /*
                 * if it isn't verbose index 2 should contain the error code.
                 */
                var errorArray = errorMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (errorArray.Length >= 3)
                {
                    if (int.TryParse(errorArray[2].Trim(), out var errorCode))
                    {
                        /*
                         * We have the error code
                         */
                        return new Tuple<int, string>(errorCode, GetMessage(errorType, errorCode));
                    }
                    /*
                     * Verbose error message without number. Return as is.
                     */
                    return new Tuple<int, string>(-1, errorMessage);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to extract {0} error code from ->{1}<-\n{2}", errorType, data, e);
            }
            return new Tuple<int, string>(-999, "Failed to extract error code.");
        }

        private static string GetMessage(ErrorType errorType, int errorCode)
        {
            LoadList();
            foreach (var errorMessage in ErrorMessageList.Where(errorMessage => errorMessage.Number == errorCode && errorMessage.ErrorType == errorType))
            {
                return errorMessage.Message;
            }
            return $"Error {errorType} : {errorCode} was not found in list.";
        }

        private static void LoadList()
        {
            if (ErrorMessageList.Count > 0)
            {
                return;
            }
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 0, "+CME ERROR 0 : phone failure"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 1, "+CME ERROR 1 : No connection to phone"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 2, "+CME ERROR 2 : phone-adaptor link reserved"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 3, "+CME ERROR 3 : operation not allowed"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 4, "+CME ERROR 4 : operation not supported"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 5, "+CME ERROR 5 : PH-SIM PIN required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 10, "+CME ERROR 10 : SIM not inserted"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 11, "+CME ERROR 11 : SIM PIN required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 12, "+CME ERROR 12 : SIM PUK required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 13, "+CME ERROR 13 : SIM failure"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 14, "+CME ERROR 14 : SIM busy"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 15, "+CME ERROR 15 : SIM wrong"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 16, "+CME ERROR 16 : incorrect password"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 17, "+CME ERROR 17 : SIM PIN2 required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 18, "+CME ERROR 18 : SIM PUK2 required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 20, "+CME ERROR 20 : memory full"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 21, "+CME ERROR 21 : invalid index"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 22, "+CME ERROR 22 : not found"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 23, "+CME ERROR 23 : memory failure"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 24, "+CME ERROR 24 : text string too long"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 25, "+CME ERROR 25 : invalid characters in text string"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 26, "+CME ERROR 26 : dial string too long"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 27, "+CME ERROR 27 : invalid characters in dial string"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 30, "+CME ERROR 30 : no network service"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 31, "+CME ERROR 31 : network time-out"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 32, "+CME ERROR 32 : network not allowed - emergency calls only"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 40, "+CME ERROR 40 : network personalization PIN required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 41, "+CME ERROR 41 : network personalization PUK required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 42, "+CME ERROR 42 : network subset personalization PIN required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 43, "+CME ERROR 43 : network subset personalization PUK required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 44, "+CME ERROR 44 : service provider personalization PIN required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 45, "+CME ERROR 45 : service provider personalization PUK required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 46, "+CME ERROR 46 : corporate personalization PIN required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 47, "+CME ERROR 47 : corporate personalization PUK required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 100, "+CME ERROR 100 : unknown"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 103, "+CME ERROR 103 : Illegal MS (#3)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 106, "+CME ERROR 106 : Illegal ME (#6)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 107, "+CME ERROR 107 : GPRS service not allowed (#7)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 111, "+CME ERROR 111 : PLMN not allowed (#11)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 112, "+CME ERROR 112 : Location area not allowed (#12)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 113, "+CME ERROR 113 : Roaming not allowed in this location area (#13)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 132, "+CME ERROR 132 : service option not supported (#32)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 133, "+CME ERROR 133 : requested service option not subscribed (#33)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 134, "+CME ERROR 134 : service option temporarily out of order (#34)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 148, "+CME ERROR 148 : unspecified GPRS error"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 149, "+CME ERROR 149 : PDP authentication failure"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 150, "+CME ERROR 150 : invalid mobile class"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 257, "+CME ERROR 257 : Network survey error (No Carrier)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 258, "+CME ERROR 258 : Network survey error (Busy)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 259, "+CME ERROR 259 : Network survey error (Wrong request)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 260, "+CME ERROR 260 : Network survey error (Aborted)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 400, "+CME ERROR 400 : generic undocumented error"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 401, "+CME ERROR 401 : wrong state"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 402, "+CME ERROR 402 : wrong mode"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 403, "+CME ERROR 403 : context already activated"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 404, "+CME ERROR 404 : stack already active"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 405, "+CME ERROR 405 : activation failed"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 406, "+CME ERROR 406 : context not opened"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 407, "+CME ERROR 407 : cannot setup socket"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 408, "+CME ERROR 408 : cannot resolve DN"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 409, "+CME ERROR 409 : time-out in opening socket"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 410, "+CME ERROR 410 : cannot open socket"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 411, "+CME ERROR 411 : remote disconnected or time-out"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 412, "+CME ERROR 412 : connection failed"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 413, "+CME ERROR 413 : tx error"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 414, "+CME ERROR 414 : already listening"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 420, "+CME ERROR 420 : ok"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 421, "+CME ERROR 421 : connect"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 422, "+CME ERROR 422 : disconnect"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 423, "+CME ERROR 423 : error"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 424, "+CME ERROR 424 : wrong state"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 425, "+CME ERROR 425 : can not activate"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 426, "+CME ERROR 426 : can not resolve name"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 427, "+CME ERROR 427 : can not allocate control socket"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 428, "+CME ERROR 428 : can not connect control socket"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 429, "+CME ERROR 429 : bad or no response from server"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 430, "+CME ERROR 430 : not connected"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 431, "+CME ERROR 431 : already connected"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 432, "+CME ERROR 432 : context down"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 433, "+CME ERROR 433 : no photo available"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 434, "+CME ERROR 434 : can not send photo"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 550, "+CME ERROR 550 : generic undocumented error"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 551, "+CME ERROR 551 : wrong state"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 552, "+CME ERROR 552 : wrong mode"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 553, "+CME ERROR 553 : context already activated"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 554, "+CME ERROR 554 : stack already active"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 555, "+CME ERROR 555 : activation failed"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 556, "+CME ERROR 556 : context not opened"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 557, "+CME ERROR 557 : cannot setup socket"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 558, "+CME ERROR 558 : cannot resolve DN"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 559, "+CME ERROR 559 : time-out in opening socket"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 560, "+CME ERROR 560 : cannot open socket"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 561, "+CME ERROR 561 : remote disconnected or time-out"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 562, "+CME ERROR 562 : connection failed"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 563, "+CME ERROR 563 : tx error"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 564, "+CME ERROR 564 : already listening"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 600, "+CME ERROR 600 : generic undocumented error"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 601, "+CME ERROR 601 : wrong state"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 602, "+CME ERROR 602 : can not activate"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 603, "+CME ERROR 603 : can not resolve name"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 604, "+CME ERROR 604 : can not allocate control socket"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 605, "+CME ERROR 605 : can not connect control socket"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 606, "+CME ERROR 606 : bad or no response from server"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 607, "+CME ERROR 607 : not connected"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 608, "+CME ERROR 608 : already connected"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 609, "+CME ERROR 609 : context down"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 610, "+CME ERROR 610 : no photo available"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 611, "+CME ERROR 611 : can not send photo"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 612, "+CME ERROR 612 : resource used by other instance"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 657, "+CME ERROR 657 : Network survey error (No Carrier)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 658, "+CME ERROR 658 : Network survey error (Busy)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 659, "+CME ERROR 659 : Network survey error (Wrong request)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 660, "+CME ERROR 660 : Network survey error (Aborted)*"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 731, "+CME ERROR 731 : Unspecified"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 732, "+CME ERROR 732 : Activation command is busy"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 733, "+CME ERROR 733 : Activation started with CMUX off"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 734, "+CME ERROR 734 : Activation started on invalid CMUX"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 736, "+CME ERROR 736 : Remote SIM already active"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CME, 737, "+CME ERROR 737 : Invalid parameter"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 0, " +CMS ERROR 0 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 1, " +CMS ERROR 1 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 2, " +CMS ERROR 2 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 3, " +CMS ERROR 3 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 4, " +CMS ERROR 4 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 5, " +CMS ERROR 5 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 6, " +CMS ERROR 6 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 7, " +CMS ERROR 7 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 8, " +CMS ERROR 8 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 9, " +CMS ERROR 9 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 10, " +CMS ERROR 10 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 11, " +CMS ERROR 11 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 12, " +CMS ERROR 12 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 13, " +CMS ERROR 13 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 14, " +CMS ERROR 14 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 15, " +CMS ERROR 15 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 16, " +CMS ERROR 16 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 17, " +CMS ERROR 17 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 18, " +CMS ERROR 18 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 19, " +CMS ERROR 19 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 20, " +CMS ERROR 20 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 21, " +CMS ERROR 21 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 22, " +CMS ERROR 22 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 23, " +CMS ERROR 23 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 24, " +CMS ERROR 24 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 25, " +CMS ERROR 25 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 26, " +CMS ERROR 26 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 27, " +CMS ERROR 27 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 28, " +CMS ERROR 28 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 29, " +CMS ERROR 29 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 30, " +CMS ERROR 30 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 31, " +CMS ERROR 31 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 32, " +CMS ERROR 32 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 33, " +CMS ERROR 33 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 34, " +CMS ERROR 34 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 35, " +CMS ERROR 35 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 36, " +CMS ERROR 36 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 37, " +CMS ERROR 37 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 38, " +CMS ERROR 38 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 39, " +CMS ERROR 39 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 40, " +CMS ERROR 40 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 41, " +CMS ERROR 41 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 42, " +CMS ERROR 42 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 43, " +CMS ERROR 43 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 44, " +CMS ERROR 44 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 45, " +CMS ERROR 45 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 46, " +CMS ERROR 46 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 47, " +CMS ERROR 47 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 48, " +CMS ERROR 48 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 49, " +CMS ERROR 49 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 50, " +CMS ERROR 50 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 51, " +CMS ERROR 51 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 52, " +CMS ERROR 52 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 53, " +CMS ERROR 53 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 54, " +CMS ERROR 54 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 55, " +CMS ERROR 55 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 56, " +CMS ERROR 56 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 57, " +CMS ERROR 57 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 58, " +CMS ERROR 58 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 59, " +CMS ERROR 59 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 60, " +CMS ERROR 60 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 61, " +CMS ERROR 61 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 62, " +CMS ERROR 62 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 63, " +CMS ERROR 63 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 64, " +CMS ERROR 64 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 65, " +CMS ERROR 65 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 66, " +CMS ERROR 66 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 67, " +CMS ERROR 67 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 68, " +CMS ERROR 68 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 69, " +CMS ERROR 69 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 70, " +CMS ERROR 70 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 71, " +CMS ERROR 71 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 72, " +CMS ERROR 72 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 73, " +CMS ERROR 73 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 74, " +CMS ERROR 74 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 75, " +CMS ERROR 75 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 76, " +CMS ERROR 76 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 77, " +CMS ERROR 77 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 78, " +CMS ERROR 78 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 79, " +CMS ERROR 79 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 80, " +CMS ERROR 80 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 81, " +CMS ERROR 81 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 82, " +CMS ERROR 82 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 83, " +CMS ERROR 83 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 84, " +CMS ERROR 84 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 85, " +CMS ERROR 85 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 86, " +CMS ERROR 86 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 87, " +CMS ERROR 87 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 88, " +CMS ERROR 88 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 89, " +CMS ERROR 89 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 90, " +CMS ERROR 90 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 91, " +CMS ERROR 91 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 92, " +CMS ERROR 92 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 93, " +CMS ERROR 93 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 94, " +CMS ERROR 94 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 95, " +CMS ERROR 95 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 96, " +CMS ERROR 96 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 97, " +CMS ERROR 97 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 98, " +CMS ERROR 98 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 99, " +CMS ERROR 99 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 100, " +CMS ERROR 100 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 101, " +CMS ERROR 101 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 102, " +CMS ERROR 102 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 103, " +CMS ERROR 103 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 104, " +CMS ERROR 104 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 105, " +CMS ERROR 105 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 106, " +CMS ERROR 106 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 107, " +CMS ERROR 107 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 108, " +CMS ERROR 108 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 109, " +CMS ERROR 109 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 110, " +CMS ERROR 110 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 111, " +CMS ERROR 111 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 112, " +CMS ERROR 112 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 113, " +CMS ERROR 113 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 114, " +CMS ERROR 114 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 115, " +CMS ERROR 115 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 116, " +CMS ERROR 116 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 117, " +CMS ERROR 117 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 118, " +CMS ERROR 118 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 119, " +CMS ERROR 119 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 120, " +CMS ERROR 120 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 121, " +CMS ERROR 121 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 122, " +CMS ERROR 122 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 123, " +CMS ERROR 123 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 124, " +CMS ERROR 124 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 125, " +CMS ERROR 125 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 126, " +CMS ERROR 126 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 127, " +CMS ERROR 127 : GSM 04.11 Annex E-2 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 128, " +CMS ERROR 128 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 129, " +CMS ERROR 129 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 130, " +CMS ERROR 130 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 131, " +CMS ERROR 131 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 132, " +CMS ERROR 132 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 133, " +CMS ERROR 133 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 134, " +CMS ERROR 134 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 135, " +CMS ERROR 135 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 136, " +CMS ERROR 136 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 137, " +CMS ERROR 137 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 138, " +CMS ERROR 138 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 139, " +CMS ERROR 139 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 140, " +CMS ERROR 140 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 141, " +CMS ERROR 141 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 142, " +CMS ERROR 142 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 143, " +CMS ERROR 143 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 144, " +CMS ERROR 144 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 145, " +CMS ERROR 145 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 146, " +CMS ERROR 146 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 147, " +CMS ERROR 147 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 148, " +CMS ERROR 148 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 149, " +CMS ERROR 149 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 150, " +CMS ERROR 150 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 151, " +CMS ERROR 151 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 152, " +CMS ERROR 152 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 153, " +CMS ERROR 153 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 154, " +CMS ERROR 154 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 155, " +CMS ERROR 155 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 156, " +CMS ERROR 156 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 157, " +CMS ERROR 157 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 158, " +CMS ERROR 158 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 159, " +CMS ERROR 159 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 160, " +CMS ERROR 160 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 161, " +CMS ERROR 161 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 162, " +CMS ERROR 162 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 163, " +CMS ERROR 163 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 164, " +CMS ERROR 164 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 165, " +CMS ERROR 165 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 166, " +CMS ERROR 166 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 167, " +CMS ERROR 167 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 168, " +CMS ERROR 168 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 169, " +CMS ERROR 169 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 170, " +CMS ERROR 170 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 171, " +CMS ERROR 171 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 172, " +CMS ERROR 172 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 173, " +CMS ERROR 173 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 174, " +CMS ERROR 174 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 175, " +CMS ERROR 175 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 176, " +CMS ERROR 176 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 177, " +CMS ERROR 177 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 178, " +CMS ERROR 178 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 179, " +CMS ERROR 179 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 180, " +CMS ERROR 180 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 181, " +CMS ERROR 181 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 182, " +CMS ERROR 182 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 183, " +CMS ERROR 183 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 184, " +CMS ERROR 184 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 185, " +CMS ERROR 185 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 186, " +CMS ERROR 186 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 187, " +CMS ERROR 187 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 188, " +CMS ERROR 188 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 189, " +CMS ERROR 189 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 190, " +CMS ERROR 190 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 191, " +CMS ERROR 191 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 192, " +CMS ERROR 192 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 193, " +CMS ERROR 193 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 194, " +CMS ERROR 194 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 195, " +CMS ERROR 195 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 196, " +CMS ERROR 196 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 197, " +CMS ERROR 197 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 198, " +CMS ERROR 198 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 199, " +CMS ERROR 199 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 200, " +CMS ERROR 200 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 201, " +CMS ERROR 201 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 202, " +CMS ERROR 202 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 203, " +CMS ERROR 203 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 204, " +CMS ERROR 204 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 205, " +CMS ERROR 205 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 206, " +CMS ERROR 206 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 207, " +CMS ERROR 207 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 208, " +CMS ERROR 208 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 209, " +CMS ERROR 209 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 210, " +CMS ERROR 210 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 211, " +CMS ERROR 211 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 212, " +CMS ERROR 212 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 213, " +CMS ERROR 213 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 214, " +CMS ERROR 214 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 215, " +CMS ERROR 215 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 216, " +CMS ERROR 216 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 217, " +CMS ERROR 217 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 218, " +CMS ERROR 218 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 219, " +CMS ERROR 219 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 220, " +CMS ERROR 220 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 221, " +CMS ERROR 221 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 222, " +CMS ERROR 222 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 223, " +CMS ERROR 223 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 224, " +CMS ERROR 224 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 225, " +CMS ERROR 225 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 226, " +CMS ERROR 226 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 227, " +CMS ERROR 227 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 228, " +CMS ERROR 228 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 229, " +CMS ERROR 229 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 230, " +CMS ERROR 230 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 231, " +CMS ERROR 231 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 232, " +CMS ERROR 232 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 233, " +CMS ERROR 233 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 234, " +CMS ERROR 234 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 235, " +CMS ERROR 235 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 236, " +CMS ERROR 236 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 237, " +CMS ERROR 237 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 238, " +CMS ERROR 238 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 239, " +CMS ERROR 239 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 240, " +CMS ERROR 240 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 241, " +CMS ERROR 241 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 242, " +CMS ERROR 242 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 243, " +CMS ERROR 243 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 244, " +CMS ERROR 244 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 245, " +CMS ERROR 245 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 246, " +CMS ERROR 246 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 247, " +CMS ERROR 247 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 248, " +CMS ERROR 248 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 249, " +CMS ERROR 249 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 250, " +CMS ERROR 250 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 251, " +CMS ERROR 251 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 252, " +CMS ERROR 252 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 253, " +CMS ERROR 253 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 254, " +CMS ERROR 254 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 255, " +CMS ERROR 255 : GSM 03.40 sub clause 9.2.3.22 values"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 300, " +CMS ERROR 300 : ME failure"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 301, " +CMS ERROR 301 : SMS service of ME reserved"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 302, " +CMS ERROR 302 : operation not allowed"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 303, " +CMS ERROR 303 : operation not supported"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 304, " +CMS ERROR 304 : invalid PDU mode parameter"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 305, " +CMS ERROR 305 : invalid text mode parameter"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 310, " +CMS ERROR 310 : SIM not inserted"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 311, " +CMS ERROR 311 : SIM PIN required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 312, " +CMS ERROR 312 : PH-SIM PIN required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 313, " +CMS ERROR 313 : SIM failure"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 314, " +CMS ERROR 314 : SIM busy"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 315, " +CMS ERROR 315 : SIM wrong"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 316, " +CMS ERROR 316 : SIM PUK required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 317, " +CMS ERROR 317 : SIM PIN2 required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 318, " +CMS ERROR 318 : SIM PUK2 required"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 320, " +CMS ERROR 320 : memory failure"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 321, " +CMS ERROR 321 : invalid memory index"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 322, " +CMS ERROR 322 : memory full"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 330, " +CMS ERROR 330 : SMSC address unknown"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 331, " +CMS ERROR 331 : no network service"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 332, " +CMS ERROR 332 : network time-out"));
            ErrorMessageList.Add(new ModemErrorMessage(ErrorType.CMS, 500, " +CMS ERROR 500 : unknown error"));
        }
    }
}
