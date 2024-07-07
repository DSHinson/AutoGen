using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAuoGenApi.models_for_auto_gen;

namespace TestAutoGen.Dal
{
    
    public class UserRepo
    {
        private readonly DalBase _baseRepo;

        public UserRepo(DalBase dalBase)
        {
            _baseRepo = dalBase ?? throw new ArgumentNullException(nameof(dalBase));
        }

        public List<User> GetAllUsers()
        {
            // Get all users
            string selectAllSql = "SELECT * FROM Users";
            var users = _baseRepo.Query<User>(selectAllSql).ToList();
            return users;
        }

        public User? GetUserById()
        {
            // Get user by Id
            string selectByIdSql = "SELECT * FROM Users WHERE Id = @Id";
            var selectByIdParameters = new { Id = 1 };
            var userById = _baseRepo.FirstOrDefault<User>(selectByIdSql, selectByIdParameters);
            return userById;
        }

        public User? UpdateUser(User user)
        {
            // Update
            string selectByIdSql = "UPDATE Users SET Name = @Name WHERE Id = @Id";
            var userById = _baseRepo.FirstOrDefault<User>(selectByIdSql, user);
            return userById;
        }

        public void DeleteUser(User user)
        {
            //Delete users
            string deleteSql = "DELETE FROM Users WHERE Id = @Id";
            _baseRepo.Execute(deleteSql, user);
        }
    }
}
