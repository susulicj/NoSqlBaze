import { Decode } from "../decode.js";
import { User } from "../UserProfile/user.js";

export class User1 {
    constructor(id, Name, LastName, Phone, UserName, Password, Email, ProfilePicture, Interests, NumberOfFriends, Status, Horoscope) {
        this.id = id;
        this.idPrijavljenogKorisnika = null;
        this.Name = Name;
        this.LastName = LastName;
        this.Phone = Phone;
        this.UserName = UserName;
        this.Password = Password;
        this.Email = Email;
        this.ProfilePicture = ProfilePicture;
        this.Interests = Interests;
        this.NumberOfFriends = NumberOfFriends;
        this.Status = Status;
        this.Horoscope = Horoscope;
        this.searchButton = document.getElementById("filtriranje");
        this.searchInput = document.getElementById("searchInput");
        this.ime = document.getElementById("ime");
        this.addRequest = document.getElementById("addRequest");
        //this.addRequest.addEventListener("click", () => this.provera())
        this.searchButton.addEventListener("click", () => this.pretraga());
         
        
    
    
    }





    

    async prikazPodataka() {
        try {

            this.dodaj();
           
            
             

            var storedSearchValue = localStorage.getItem('searchValue');
            console.log(storedSearchValue);

            const response = await fetch(`http://localhost:5142/User/getUserByUsername/${storedSearchValue}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                },
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            if (response.status === 204) {
                this.ime.innerHTML = 'Ne postoji user';
                throw new Error('No data found');
            }

            const data = await response.json();
            console.log(data);

            this.ime.innerText = data.name + " " + data.lastName;
            this.id = data.id;
            this.ProfilePicture = data.profilePicture;
            this.displayImage(this.ProfilePicture);
            console.log(data);
            var interesovanja = document.getElementById("interesovanja");
            console.log(data.interests)
            if (data.interests != undefined) {
                interesovanja.innerText = "Interesovanja:" + " " + data.interests;
            }

            var Horoscope = document.getElementById("horoskop");
            if (data.horoscope != undefined) {
                Horoscope.innerText = "Horoskop:" + " " + data.horoscope;
                console.log('User data:', data);
            }

            var brojPrijatelja = document.getElementById("brojPrijatelja");
                console.log(data.numbersOfFriends);
                brojPrijatelja.innerText = "Broj prijatelja:" + " "+ data.numbersOfFriends;
                console.log('User data:', data);
            

            var email = document.getElementById("email");
                if (data.email != undefined) {
                    this.email = data.email;
                    email.innerText = "Email:"+" "+data.email;
                    console.log('User data:', data);
                }
              
                    var username = document.getElementById("username");
                    
                    if (data.userName != undefined) {
                        
                        username.innerText = data.userName;
                    
                    }

            console.log('User data:', data);
        } catch (error) {
            console.error('Error while fetching user data:', error.message);
        }


        this.provera();
    }
    
    displayImage(putanja) {
     
        var imgElement = document.getElementById('prikaziSliku');
       
        imgElement.src = putanja;
    }
    pretraga() {
        console.log("pritisnula sam");
        var searchInput = document.getElementById("searchInput");
        var searchValue =  searchInput.value;
        localStorage.setItem('searchValue', searchValue);
        console.log(searchValue);

        window.location.href =  window.location.href = location.origin + '/Front/DrugiKorisnici/profile1.html';


    }
    async someAsyncFunction() {
        try {
             
            var decode  = new Decode();
            const user = await decode.vratiKorisnika();
            this.idPrijavljenogKorisnika = user;
            
        } catch (error) {
            console.error('Error:', error);
        }
    }
    




    async posaljiZahtev(){
        
       
        var idProfila = this.id;
        await this.someAsyncFunction();

        
        console.log(idProfila);
        console.log(this.idPrijavljenogKorisnika.id);

         
        await fetch(`http://localhost:5142/User/addFriend/${this.idPrijavljenogKorisnika.id}/${idProfila}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
               
            },
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.text(); 
            })
            .then(data => {
                console.log(data); 
                location.reload();
            })
            .catch(error => {
                console.error('Error while adding friend:', error);
            });

        
         

    }


    async proveriPrijateljstvo()
    {
        await this.someAsyncFunction();
        try {
            console.log(this.idPrijavljenogKorisnika);
            console.log(this.id);
            const response = await fetch(`http://localhost:5142/User/checkFriendship/${this.idPrijavljenogKorisnika.id}/${this.id}`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                   
                },
            });
    
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
    
            const data = await response.text();
            console.log(data); 
            return data;
            
    
            
            return data;
        } catch (error) {
            console.error('Error while checking friendship:', error);
        }
    }

    async provera(){

        
        var statusPrijateljstva = await this.proveriPrijateljstvo();
        console.log(statusPrijateljstva);

        if(statusPrijateljstva == 1){
            this.addRequest.innerHTML = "Ukloni prijatelja";
            this.addRequest.addEventListener("click", () => this.obrisiPrijatelja());
        }
        else if(statusPrijateljstva == 2){
            this.addRequest.innerHTML = "Zahtev poslat";
            
        }
        else if(statusPrijateljstva == 3){
            this.addRequest.innerHTML = "Posalji zahtev";
            this.addRequest.addEventListener("click", () => this.posaljiZahtev());
        
        }

        


    }

    async obrisiPrijatelja(){
        try {
            await this.someAsyncFunction();
            const response = await fetch(`http://localhost:5142/User/deleteFriend/${this.id}/${this.idPrijavljenogKorisnika.id}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                   
                },
            });
    
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
    
            const data = await response.text();
            console.log(data); 
            location.reload();
            return data;
            
    
            
            return data;
        } catch (error) {
            console.error('Error while checking friendship:', error);
        }
    }

    async dodaj()
    {
        var korisnikid = localStorage.getItem('searchValue');
        var contentDiv = document.getElementById("dodaj");
        const storyIframe = document.createElement('iframe');
        console.log(this.id);
        storyIframe.src = `../HighlightsKorisnici/highlightKorisnici.html?dataId=${korisnikid}`;  //odje menjas
        storyIframe.style.width = '100%';
        storyIframe.style.height = '900px'; // Set the height as needed

              

        //contentDiv.appendChild(heading);
        //contentDiv.appendChild(description);
        contentDiv.appendChild(storyIframe);
    }


}
