namespace DOTNETAPI
{

    public partial class UsersJobInfo{

        public int UserId {get; set;}
        public string JobTitle {get; set;}
        public string Department {get; set;}
       


        public UsersJobInfo()
        {
            if(JobTitle == null){
                JobTitle = " ";
            }
            if(Department == null){
                Department = " ";
            }

        }

    }

}