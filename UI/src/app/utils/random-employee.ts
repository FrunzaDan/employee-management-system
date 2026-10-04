import { Address, Gender } from '../interfaces/employee';

// Test employees look like staff of a Romanian company: working-age adults,
// Romanian names mixed with the Hungarian and Saxon names of Transylvania,
// living mostly in the big cities where the offices are.

const MALE_FIRST_NAMES = [
  'Andrei',
  'Alexandru',
  'Mihai',
  'Ion',
  'Cristian',
  'Florin',
  'Radu',
  'George',
  'Dan',
  'Adrian',
  'Bogdan',
  'Cătălin',
  'Ștefan',
  'Vlad',
  'Gabriel',
  'Răzvan',
  'Sorin',
  'Marius',
  'Ionuț',
  'Ciprian',
  'Lucian',
  'Ovidiu',
  'Cosmin',
  'Daniel',
  'Sebastian',
  'Tudor',
  'Victor',
  'Octavian',
  'Liviu',
  'Paul',
  'Emil',
  'Silviu',
  'Attila',
  'Zoltán',
  'Csaba',
  'Levente',
  'Tamás',
  'Hans',
  'Klaus',
  'Stefan',
];

const FEMALE_FIRST_NAMES = [
  'Maria',
  'Elena',
  'Ioana',
  'Ana',
  'Gabriela',
  'Andreea',
  'Simona',
  'Cristina',
  'Diana',
  'Larisa',
  'Mihaela',
  'Roxana',
  'Monica',
  'Alina',
  'Nicoleta',
  'Laura',
  'Oana',
  'Raluca',
  'Irina',
  'Adina',
  'Corina',
  'Bianca',
  'Alexandra',
  'Carmen',
  'Daniela',
  'Iulia',
  'Teodora',
  'Anca',
  'Ramona',
  'Ana-Maria',
  'Georgiana',
  'Denisa',
  'Enikő',
  'Katalin',
  'Réka',
  'Noémi',
  'Orsolya',
  'Ingrid',
  'Renate',
  'Sabine',
];

const LAST_NAMES = [
  'Popescu',
  'Ionescu',
  'Popa',
  'Dumitrescu',
  'Stan',
  'Gheorghiu',
  'Constantinescu',
  'Marinescu',
  'Stoica',
  'Matei',
  'Ciobanu',
  'Munteanu',
  'Rusu',
  'Florea',
  'Nistor',
  'Oprea',
  'Cristea',
  'Preda',
  'Dobre',
  'Sârbu',
  'Neagu',
  'Bălan',
  'Moldovan',
  'Dragomir',
  'Suciu',
  'Ungureanu',
  'Vlad',
  'Mocanu',
  'Pop',
  'Crișan',
  'Mureșan',
  'Rus',
  'Kovács',
  'Szabó',
  'Nagy',
  'Tóth',
  'Farkas',
  'Molnár',
  'Schuster',
  'Wagner',
  'Fleischer',
  'Roth',
];

// Heavier weights on the cities with offices, so most staff live near one.
const CITIES: readonly {
  county: string;
  city: string;
  postalPrefix: string;
  weight: number;
}[] = [
  { county: 'București', city: 'București', postalPrefix: '0', weight: 14 },
  { county: 'Cluj', city: 'Cluj-Napoca', postalPrefix: '400', weight: 12 },
  { county: 'Iași', city: 'Iași', postalPrefix: '700', weight: 8 },
  { county: 'Timiș', city: 'Timișoara', postalPrefix: '300', weight: 8 },
  { county: 'Brașov', city: 'Brașov', postalPrefix: '500', weight: 6 },
  { county: 'Ilfov', city: 'Voluntari', postalPrefix: '077', weight: 4 },
  { county: 'Ilfov', city: 'Otopeni', postalPrefix: '075', weight: 3 },
  { county: 'Cluj', city: 'Florești', postalPrefix: '407', weight: 3 },
  { county: 'Sibiu', city: 'Sibiu', postalPrefix: '550', weight: 3 },
  { county: 'Constanța', city: 'Constanța', postalPrefix: '900', weight: 3 },
  { county: 'Bihor', city: 'Oradea', postalPrefix: '410', weight: 3 },
  { county: 'Mureș', city: 'Târgu Mureș', postalPrefix: '540', weight: 2 },
  {
    county: 'Harghita',
    city: 'Miercurea Ciuc',
    postalPrefix: '530',
    weight: 1,
  },
  {
    county: 'Covasna',
    city: 'Sfântu Gheorghe',
    postalPrefix: '520',
    weight: 1,
  },
  { county: 'Dolj', city: 'Craiova', postalPrefix: '200', weight: 2 },
  { county: 'Prahova', city: 'Ploiești', postalPrefix: '100', weight: 2 },
  { county: 'Arad', city: 'Arad', postalPrefix: '310', weight: 1 },
  { county: 'Alba', city: 'Alba Iulia', postalPrefix: '510', weight: 1 },
];

const STREETS = [
  'Strada Avram Iancu',
  'Strada Memorandumului',
  'Bulevardul Eroilor',
  'Bulevardul 21 Decembrie 1989',
  'Calea Dorobanților',
  'Calea Victoriei',
  'Bulevardul Unirii',
  'Strada Observatorului',
  'Strada Republicii',
  'Aleea Tineretului',
  'Strada Fabricii',
  'Bulevardul Independenței',
  'Strada Mihai Eminescu',
  'Strada Garoafelor',
  'Calea Aradului',
  'Strada Lalelelor',
  'Bulevardul Tudor Vladimirescu',
  'Strada Morii',
  'Strada Zorilor',
  'Aleea Parcului',
];

const EMAIL_DOMAINS = ['example.com', 'example.net', 'mail.example.org'];

export const GENDER_WEIGHTS: readonly { gender: Gender; weight: number }[] = [
  { gender: Gender.Male, weight: 46 },
  { gender: Gender.Female, weight: 46 },
  { gender: Gender.NotDeclared, weight: 8 },
];

export function pick<T>(values: readonly T[]): T {
  return values[Math.floor(Math.random() * values.length)];
}

export function pickWeighted<T extends { weight: number }>(
  values: readonly T[],
): T {
  const total = values.reduce((sum, value) => sum + value.weight, 0);
  let roll = Math.random() * total;
  for (const value of values) {
    roll -= value.weight;
    if (roll < 0) return value;
  }
  return values[values.length - 1];
}

export function randomInt(min: number, max: number): number {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

function randomDigits(length: number): string {
  let digits = '';
  for (let i = 0; i < length; i++) {
    digits += Math.floor(Math.random() * 10).toString();
  }
  return digits;
}

export function toIsoDate(date: Date): string {
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const day = date.getDate().toString().padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

function randomDateBetween(start: Date, end: Date): Date {
  return new Date(
    start.getTime() + Math.random() * (end.getTime() - start.getTime()),
  );
}

function forEmail(name: string): string {
  return name
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .replace(/[^a-z-]/g, '');
}

// Generated records carry this word after their names, so they are easy to
// tell apart from real ones and to find again.
const TEST_SUFFIX = 'Test';

export function randomGender(): Gender {
  return pickWeighted(GENDER_WEIGHTS).gender;
}

function firstNameFor(gender: Gender): string {
  if (gender === Gender.Male) return pick(MALE_FIRST_NAMES);
  if (gender === Gender.Female) return pick(FEMALE_FIRST_NAMES);
  return pick(Math.random() < 0.5 ? MALE_FIRST_NAMES : FEMALE_FIRST_NAMES);
}

// Tries a few spellings until one is not in `taken`, then claims it.
function uniqueValue(taken: Set<string>, candidate: () => string): string {
  let value = candidate();
  while (taken.has(value)) value = candidate();
  taken.add(value);
  return value;
}

function randomEmail(
  firstName: string,
  lastName: string,
  birthYear: number,
): string {
  const first = forEmail(firstName);
  const last = forEmail(lastName);
  const local = pick([
    `${first}.${last}`,
    `${first[0]}.${last}`,
    `${first}${last}`,
    `${last}.${first}`,
    `${first}_${last}`,
    `${first}.${last}${birthYear % 100}`,
  ]);
  const number = Math.random() < 0.6 ? randomInt(1, 999).toString() : '';
  return `${local}${number}@${pick(EMAIL_DOMAINS)}`;
}

function randomPhoneNumber(): string {
  return `07${randomInt(2, 9)}${randomDigits(7)}`;
}

export interface RandomEmployee {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  gender: Gender;
  birthDate: string;
  hireDate: string;
  address: Address;
}

// Hired between the given dates, aged 19 to 55 on the hire date. `taken`
// holds the emails and phone numbers already used in this run.
export function buildRandomEmployee(
  hireFrom: Date,
  hireTo: Date,
  taken: Set<string>,
): RandomEmployee {
  const gender = randomGender();
  const firstName = firstNameFor(gender);
  const lastName =
    gender === Gender.Female && Math.random() < 0.1
      ? `${pick(LAST_NAMES)}-${pick(LAST_NAMES)}`
      : pick(LAST_NAMES);

  const hired = randomDateBetween(hireFrom, hireTo);
  const born = new Date(hired);
  born.setFullYear(hired.getFullYear() - randomInt(19, 55));
  born.setDate(born.getDate() - randomInt(0, 364));

  const { county, city, postalPrefix } = pickWeighted(CITIES);

  return {
    firstName: firstName + TEST_SUFFIX,
    lastName: lastName + TEST_SUFFIX,
    email: uniqueValue(taken, () =>
      randomEmail(firstName, lastName, born.getFullYear()),
    ),
    phoneNumber: uniqueValue(taken, randomPhoneNumber),
    gender,
    birthDate: toIsoDate(born),
    hireDate: toIsoDate(hired),
    address: {
      country: 'Romania',
      county,
      city,
      street: pick(STREETS),
      streetNumber: `${randomInt(1, 180)}${Math.random() < 0.1 ? pick(['A', 'B']) : ''}`,
      postalCode: `${postalPrefix}${randomDigits(6 - postalPrefix.length)}`,
    },
  };
}
