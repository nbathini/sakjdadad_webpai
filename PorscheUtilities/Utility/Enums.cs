namespace PorscheUtilities.Utility
{
    public class Enums
    {
        public enum QuestionType
        {
            DeliveryPreparation = 1,            
            CustomerSurvey,
            PreDeliveryCheckList,
            DeliveryCheckList,
            Infosheet,
            SecondDeliveryCheckList,
        }
        public enum Designation
        {
            PorshePro = 1,
            ServiceAdvisor,
            SalesConsultant,
            SalesManager,
            ServiceManager,
            GeneralManager,
        }
        
        public enum UserRole
        {
            Staff = 1,
            Centre,
            SuperAdmin
        }
        public enum DeliveryType
        {
            PreDelivery = 1,
            PostDelivery
        }

        public enum DeliveryStatus
        {
            DeliveryPending=1,
            CustomerFollowUpPending,
            DeliveryDone
        }
        public enum HomeChargerStatus
        {
            NotAvailable = 1,
            NotStarted,
            InProgress,
            Completed
        }
    }
}
