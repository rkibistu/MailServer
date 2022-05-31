using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client {

    public class Message {


        public Message() {

            m_parameters = new List<string>();
        }
        public byte m_instruction;
        public List<string> m_parameters;
        public void Construct(byte instruction, params string[] parameters) {

            m_instruction = instruction;

            m_parameters.Clear();
            foreach (string param in parameters) {

                m_parameters.Add(param);
            }
        }
        public void Print() {

            foreach (string param in m_parameters) {

                Console.Write(param + " ");
            }
            Console.WriteLine();
        }

    }
    public static class Protocol {

        //forma mesaj:
        //instructiune(1byte) numberOfParameters(1byte) lenhtParam1(1byte) param1(63bytes) lenghtParam2(1byte) param2(63bytes) ....

        //constante
        public const int MESSAGE_SIZE = 10025;
        private const int PARAMETER_SIZE = 64; //maxim 255

        //lista raspunsuri
        public const byte FAILED = 0;
        public const byte SUCCES = 1;

        //lista comenzi
        public const byte Login = 100;
        public const byte MovieDetails = 101;


        static public byte[] ConvertToBytes(Message message) {

            byte[] bytes = new byte[MESSAGE_SIZE];
            int index = 0;
            bytes[index++] = message.m_instruction;

            bytes[index++] = BitConverter.GetBytes(message.m_parameters.Count)[0];

            byte[] tempBytes;
            foreach (string parametru in message.m_parameters) {

                tempBytes = System.Text.Encoding.ASCII.GetBytes(parametru);
                bytes[index++] = BitConverter.GetBytes(tempBytes.Length)[0]; //aici putem cu [0] pt ca stim ca nici un parametru nu e mai lung de 255
                Array.Copy(tempBytes, 0, bytes, index, tempBytes.Length);
                index += PARAMETER_SIZE - 1;
            }

            return bytes;
        }
        static public Message ConvertFromBytes(byte[] bytes) {

            int index = 0;

            Message response = new Message();
            response.m_instruction = bytes[index++]; //preluam instructiunea/raspunsul

            int numberOfParameters = bytes[index++]; //vedem cati parametrii s au trimis
            for (int i = 0; i < numberOfParameters; i++) {

                int parameterLenght = bytes[index++]; //luam lungimea in bytes a parametruui curent
                byte[] tempBytes = new byte[parameterLenght]; // ne declaram un vector temporar de bytes
                Array.Copy(bytes, index, tempBytes, 0, parameterLenght); //copiem in el parametrul curent
                string tempString = System.Text.Encoding.ASCII.GetString(tempBytes); // il convertmin la string
                response.m_parameters.Add(tempString); //adaugam stringul la raspuns (clasa mesaj)

                index += PARAMETER_SIZE - 1; //crestem indexul (scadem 1 pentru ca am adunat 1 cand am preluat parameterLenght)
            }

            return response;
        }
    }
}
