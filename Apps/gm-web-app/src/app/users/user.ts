export enum UserRole {
    SuperUser = 0,
    Administrator = 1,
    Default = 2
}

export class User {
    constructor(public id: number,
        public firstName: string,
        public lastName: string,
        public username: string,
        public password: string,
        public role: UserRole) {
    }

    public authdata!: string;

    public isEmpty() {
        return !this.isValid();
    }

    public isValid() {
        return this.username != '';
    }
}
